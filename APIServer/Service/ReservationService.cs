using APIServer.Data;
using APIServer.DTO.Reservations;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class ReservationService : IReservationService
    {
        private readonly LibraryDatabaseContext _context;

        // FIX 1: Add lock for thread safety,
        private static readonly object _expirationLock = new object();
        private static DateTime LastExpiredReservationCheck = DateTime.MinValue;

        public ReservationService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Reservation?> CreateReservationAsync(ReservationCreateDTO dto)
        {
            // FIX 2: Add validation
            if (dto.ExpirationDate.HasValue && dto.ExpirationDate.Value <= DateTime.Now)
            {
                throw new ArgumentException("Expiration date must be in the future");
            }

            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
            if (!userExists)
            {
                throw new ArgumentException($"User {dto.UserId} not found");
            }

            // Check if variant exists
            var variantExists = await _context.BookVariants.AnyAsync(v => v.VariantId == dto.VariantId);
            if (!variantExists)
            {
                throw new ArgumentException($"Book variant {dto.VariantId} not found");
            }

            // Check if user has unpaid fines
            var hasUnpaidFines = await _context.Loans
                .Where(l => l.UserId == dto.UserId &&
                           (l.LoanStatus == "Overdue" || (l.FineAmount > 0 && l.LoanStatus != "Returned")))
                .AnyAsync();

            if (hasUnpaidFines)
            {
                return null; // User has unpaid fines
            }

            // Check if user already has a reservation for this variant
            var existingReservation = await _context.Reservations
                .Where(r => r.UserId == dto.UserId &&
                           r.VariantId == dto.VariantId &&
                           r.ReservationStatus == "Pending")
                .FirstOrDefaultAsync();

            if (existingReservation != null)
            {
                return null; // User already has pending reservation for this book
            }

            // Check if book is available (no need to reserve if available)
            var availableCopies = await _context.BookCopies
                .Where(c => c.VariantId == dto.VariantId && c.CopyStatus == "Available")
                .CountAsync();

            if (availableCopies > 0)
            {
                return null; // Book is available, no need to reserve
            }

            // FIX 3: Add transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create reservation
                var reservation = new Reservation
                {
                    UserId = dto.UserId,
                    VariantId = dto.VariantId,
                    ReservationDate = DateTime.Now,
                    ExpirationDate = dto.ExpirationDate ?? DateTime.Now.AddDays(7),
                    ReservationStatus = "Pending"
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return reservation;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelReservationAsync(int reservationId, int userId)
        {
            var reservation = await _context.Reservations
                .Where(r => r.ReservationId == reservationId &&
                           r.UserId == userId &&
                           r.ReservationStatus == "Pending")
                .FirstOrDefaultAsync();

            if (reservation == null)
                return false;

            reservation.ReservationStatus = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelReservationByStaffAsync(int reservationId, int staffId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null || reservation.ReservationStatus != "Pending")
                return false;

            reservation.ReservationStatus = "Cancelled";
            reservation.ProcessedBy = staffId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReservationListDTO>> GetAllReservationsListAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? status = null)
        {
            // FIX 4: Ensure page and pageSize are valid
            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(100, pageSize)); // Max 100 items per page

            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Variant.Volume.Book.Authors)
                .Include(r => r.Variant.Publisher)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r =>
                    r.Variant.Volume.Book.Title.Contains(keyword) ||
                    r.User.FullName.Contains(keyword) ||
                    r.User.Username.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.ReservationStatus == status);
            }

            var reservations = await query
                .OrderBy(r => r.ReservationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTO in memory
            return reservations.Select((r, index) => new ReservationListDTO
            {
                ReservationId = r.ReservationId,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                ReservationStatus = r.ReservationStatus,
                QueuePosition = ((page - 1) * pageSize) + index + 1,

                Username = r.User.Username,
                FullName = r.User.FullName,
                Email = r.User.Email,

                Title = r.Variant.Volume.Book.Title,
                VolumeTitle = r.Variant.Volume.VolumeTitle ?? "",
                Authors = r.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),
                PublisherName = r.Variant.Publisher.PublisherName,
                ISBN = r.Variant.Isbn ?? "",

                TotalCopies = 0, // Will calculate if needed
                AvailableCopies = 0
            }).ToList();
        }

        public async Task<List<ReservationListDTO>> GetUserReservationsAsync(int userId)
        {
            // Get reservations first
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Variant)
                    .ThenInclude(v => v.Volume)
                        .ThenInclude(vol => vol.Book)
                            .ThenInclude(b => b.Authors)
                .Include(r => r.Variant.Publisher)
                .Where(r => r.UserId == userId && r.ReservationStatus == "Pending")
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            // Get copy stats separately
            var variantIds = reservations.Select(r => r.VariantId).Distinct().ToList();
            var copyStats = await _context.BookCopies
                .Where(c => variantIds.Contains(c.VariantId))
                .GroupBy(c => c.VariantId)
                .Select(g => new {
                    VariantId = g.Key,
                    TotalCopies = g.Count(),
                    AvailableCopies = g.Count(c => c.CopyStatus == "Available")
                })
                .ToListAsync();

            // Map to DTO in memory
            var result = new List<ReservationListDTO>();
            foreach (var r in reservations)
            {
                var queuePosition = await _context.Reservations
                    .Where(res => res.VariantId == r.VariantId &&
                                 res.ReservationStatus == "Pending" &&
                                 res.ReservationDate < r.ReservationDate)
                    .CountAsync() + 1;

                var copyInfo = copyStats.FirstOrDefault(c => c.VariantId == r.VariantId);

                result.Add(new ReservationListDTO
                {
                    ReservationId = r.ReservationId,
                    ReservationDate = r.ReservationDate,
                    ExpirationDate = r.ExpirationDate,
                    ReservationStatus = r.ReservationStatus,
                    QueuePosition = queuePosition,

                    Username = r.User.Username,
                    FullName = r.User.FullName,
                    Email = r.User.Email,

                    Title = r.Variant.Volume.Book.Title,
                    VolumeTitle = r.Variant.Volume.VolumeTitle ?? "",
                    Authors = r.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),
                    PublisherName = r.Variant.Publisher.PublisherName,
                    ISBN = r.Variant.Isbn ?? "",

                    TotalCopies = copyInfo?.TotalCopies ?? 0,
                    AvailableCopies = copyInfo?.AvailableCopies ?? 0
                });
            }

            return result;
        }

        public IQueryable<ReservationListDTO> GetAllReservations()
        {
            // FIX 5: Thread-safe expiration check
            lock (_expirationLock)
            {
                if (LastExpiredReservationCheck.Date < DateTime.Today)
                {
                    ProcessExpiredReservationsAsync().Wait(); // Not ideal but OK for MVP
                    LastExpiredReservationCheck = DateTime.Today;
                }
            }

            return _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Variant)
                    .ThenInclude(v => v.Volume)
                        .ThenInclude(vol => vol.Book)
                            .ThenInclude(b => b.Authors)
                .Include(r => r.Variant.Publisher)
                .Select(r => new ReservationListDTO
                {
                    ReservationId = r.ReservationId,
                    ReservationDate = r.ReservationDate,
                    ExpirationDate = r.ExpirationDate,
                    ReservationStatus = r.ReservationStatus,
                    QueuePosition = _context.Reservations
                        .Where(res => res.VariantId == r.VariantId &&
                                     res.ReservationStatus == "Pending" &&
                                     res.ReservationDate < r.ReservationDate)
                        .Count() + 1,

                    Username = r.User.Username,
                    FullName = r.User.FullName,
                    Email = r.User.Email,

                    Title = r.Variant.Volume.Book.Title,
                    VolumeTitle = r.Variant.Volume.VolumeTitle ?? "",
                    Authors = r.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),
                    PublisherName = r.Variant.Publisher.PublisherName,
                    ISBN = r.Variant.Isbn ?? "",

                    TotalCopies = _context.BookCopies.Where(c => c.VariantId == r.VariantId).Count(),
                    AvailableCopies = _context.BookCopies
                        .Where(c => c.VariantId == r.VariantId && c.CopyStatus == "Available")
                        .Count()
                });
        }

        public async Task<int> GetReservationsCountAsync(string? keyword)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Variant)
                    .ThenInclude(v => v.Volume)
                        .ThenInclude(vol => vol.Book)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r =>
                    r.Variant.Volume.Book.Title.Contains(keyword) ||
                    r.User.FullName.Contains(keyword) ||
                    r.User.Username.Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<ReservationListDTO?> GetReservationByIdAsync(int reservationId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Variant)
                    .ThenInclude(v => v.Volume)
                        .ThenInclude(vol => vol.Book)
                            .ThenInclude(b => b.Authors)
                .Include(r => r.Variant.Publisher)
                .Where(r => r.ReservationId == reservationId)
                .Select(r => new ReservationListDTO
                {
                    ReservationId = r.ReservationId,
                    ReservationDate = r.ReservationDate,
                    ExpirationDate = r.ExpirationDate,
                    ReservationStatus = r.ReservationStatus,
                    QueuePosition = _context.Reservations
                        .Where(res => res.VariantId == r.VariantId &&
                                     res.ReservationStatus == "Pending" &&
                                     res.ReservationDate < r.ReservationDate)
                        .Count() + 1,

                    Username = r.User.Username,
                    FullName = r.User.FullName,
                    Email = r.User.Email,

                    VariantId = r.Variant.VariantId,
                    Title = r.Variant.Volume.Book.Title,
                    VolumeTitle = r.Variant.Volume.VolumeTitle ?? "",
                    Authors = r.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),
                    PublisherName = r.Variant.Publisher.PublisherName,
                    ISBN = r.Variant.Isbn ?? "",

                    TotalCopies = _context.BookCopies.Where(c => c.VariantId == r.VariantId).Count(),
                    AvailableCopies = _context.BookCopies
                        .Where(c => c.VariantId == r.VariantId && c.CopyStatus == "Available")
                        .Count()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateReservationAsync(int reservationId, ReservationUpdateDTO dto)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return false;

            if (!string.IsNullOrEmpty(dto.ReservationStatus))
                reservation.ReservationStatus = dto.ReservationStatus;

            if (dto.ExpirationDate.HasValue)
                reservation.ExpirationDate = dto.ExpirationDate.Value;

            if (dto.ProcessedBy.HasValue)
                reservation.ProcessedBy = dto.ProcessedBy.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BookAvailabilityDTO?> GetBookAvailabilityAsync(int variantId)
        {
            return await _context.BookVariants
                .Include(v => v.Volume)
                    .ThenInclude(vol => vol.Book)
                        .ThenInclude(b => b.Authors)
                .Include(v => v.Publisher)
                .Where(v => v.VariantId == variantId)
                .Select(v => new BookAvailabilityDTO
                {
                    VariantId = v.VariantId,
                    Title = v.Volume.Book.Title,
                    VolumeTitle = v.Volume.VolumeTitle ?? "",
                    Authors = v.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),
                    PublisherName = v.Publisher.PublisherName,
                    ISBN = v.Isbn ?? "",
                    TotalCopies = _context.BookCopies.Where(c => c.VariantId == variantId).Count(),
                    AvailableCopies = _context.BookCopies
                        .Where(c => c.VariantId == variantId && c.CopyStatus == "Available")
                        .Count(),
                    PendingReservations = _context.Reservations
                        .Where(r => r.VariantId == variantId && r.ReservationStatus == "Pending")
                        .Count(),
                    CanReserve = _context.BookCopies
                        .Where(c => c.VariantId == variantId && c.CopyStatus == "Available")
                        .Count() == 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetQueuePositionAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return 0;

            return await _context.Reservations
                .Where(r => r.VariantId == reservation.VariantId &&
                           r.ReservationStatus == "Pending" &&
                           r.ReservationDate < reservation.ReservationDate)
                .CountAsync() + 1;
        }

        public async Task<List<ReservationListDTO>> GetReservationQueueAsync(int variantId)
        {
            // Get reservations first
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Variant.Volume.Book.Authors)
                .Include(r => r.Variant.Publisher)
                .Where(r => r.VariantId == variantId && r.ReservationStatus == "Pending")
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            // Get copy stats separately  
            var copyStats = await _context.BookCopies
                .Where(c => c.VariantId == variantId)
                .GroupBy(c => c.CopyStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalCopies = copyStats.Sum(s => s.Count);
            var availableCopies = copyStats.Where(s => s.Status == "Available").Sum(s => s.Count);

            // Map in memory
            return reservations.Select((r, index) => new ReservationListDTO
            {
                ReservationId = r.ReservationId,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                ReservationStatus = r.ReservationStatus,
                QueuePosition = index + 1,
                Username = r.User.Username,
                FullName = r.User.FullName,
                Email = r.User.Email,
                Title = r.Variant.Volume.Book.Title,
                VolumeTitle = r.Variant.Volume.VolumeTitle ?? "",
                Authors = r.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),
                PublisherName = r.Variant.Publisher.PublisherName,
                ISBN = r.Variant.Isbn ?? "",
                TotalCopies = totalCopies,
                AvailableCopies = availableCopies
            }).ToList();
        }

        public async Task ProcessExpiredReservationsAsync()
        {
            try
            {
                var expiredReservations = await _context.Reservations
                    .Where(r => r.ReservationStatus == "Pending" &&
                               r.ExpirationDate.HasValue &&
                               r.ExpirationDate.Value < DateTime.Now)
                    .ToListAsync();

                foreach (var reservation in expiredReservations)
                {
                    reservation.ReservationStatus = "Expired";
                }

                if (expiredReservations.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                Console.WriteLine($"Error processing expired reservations: {ex.Message}");
            }
        }

        public async Task<Reservation?> GetNextAvailableReservationAsync(int variantId)
        {
            return await _context.Reservations
                .Where(r => r.VariantId == variantId && r.ReservationStatus == "Pending")
                .OrderBy(r => r.ReservationDate)
                .FirstOrDefaultAsync();
        }

        public async Task NotifyNextReservationAsync(int variantId)
        {
            var nextReservation = await GetNextAvailableReservationAsync(variantId);
            if (nextReservation != null)
            {
                // TODO: Implement notification logic
                Console.WriteLine($"Book available for reservation {nextReservation.ReservationId}");

                // Update expiration date to give user time to borrow
                nextReservation.ExpirationDate = DateTime.Now.AddDays(3);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasPendingReservationsAsync(int variantId)
        {
            return await _context.Reservations
                .AnyAsync(r => r.VariantId == variantId && r.ReservationStatus == "Pending");
        }
    }
}