using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.DTO.Loan;
using APIServer.DTO.Loans;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class LoanService : ILoanService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly IReservationService _reservationService;
        private readonly IEmailService _emailService;

        // FIX 1: Add lock for thread safety.
        private static readonly object _penaltyLock = new object();
        private static DateTime LastPenaltyUpdateDate = DateTime.MinValue;

        public LoanService(LibraryDatabaseContext context, IReservationService reservationService, IEmailService emailService)
        {
            _context = context;
            _reservationService = reservationService;
            _emailService = emailService;
        }

        // FIX 2: Add validation and better error handling
        public async Task<LoanListDTO?> CreateLoanAsync(LoanCreateDTO dto)
        {
            // Validate input
            if (dto.DueDate <= DateTime.Now)
            {
                throw new ArgumentException("Due date must be in the future");
            }

            var copy = await _context.BookCopies.FindAsync(dto.CopyId);
            if (copy == null)
            {
                throw new ArgumentException($"Book copy {dto.CopyId} not found");
            }

            if (copy.CopyStatus != "Available")
            {
                return null;
            }

            var hasUnpaidFines = await _context.Loans
                .Where(l => l.UserId == dto.UserId &&
                           (l.LoanStatus == "Overdue" || (l.FineAmount > 0 && l.LoanStatus != "Returned")))
                .AnyAsync();

            if (hasUnpaidFines)
            {
                return null;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var loan = new Loan
                {
                    UserId = dto.UserId,
                    CopyId = dto.CopyId,
                    BorrowDate = DateTime.Now,
                    DueDate = dto.DueDate,
                    LoanStatus = "Borrowed",
                    ReservationId = dto.ReservationId,
                    FineAmount = 0,
                    Extended = false
                };

                _context.Loans.Add(loan);
                copy.CopyStatus = "Borrowed";

                if (dto.ReservationId.HasValue)
                {
                    var reservation = await _context.Reservations.FindAsync(dto.ReservationId.Value);
                    if (reservation != null)
                    {
                        reservation.ReservationStatus = "Fulfilled";
                        reservation.FulfilledCopyId = dto.CopyId;
                }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load full entity for DTO mapping
                var fullLoan = await _context.Loans
                    .Include(l => l.User)
                    .Include(l => l.Copy)
                        .ThenInclude(c => c.Variant)
                            .ThenInclude(v => v.Volume)
                                .ThenInclude(vol => vol.Book)
                                    .ThenInclude(book => book.Authors)
                    .FirstOrDefaultAsync(l => l.LoanId == loan.LoanId);

                if (fullLoan == null)
                    return null;

                return new LoanListDTO
                {
                    LoanId = fullLoan.LoanId,
                    BorrowDate = fullLoan.BorrowDate,
                    DueDate = fullLoan.DueDate,
                    ReturnDate = fullLoan.ReturnDate,
                    LoanStatus = fullLoan.LoanStatus,
                    FineAmount = fullLoan.FineAmount ?? 0,
                    Extended = fullLoan.Extended ?? false,

                    Username = fullLoan.User.Username,
                    FullName = fullLoan.User.FullName,
                    Email = fullLoan.User.Email,

                    Barcode = fullLoan.Copy.Barcode,
                    Title = fullLoan.Copy.Variant.Volume.Book.Title,
                    VolumeTitle = fullLoan.Copy.Variant.Volume.VolumeTitle ?? "",
                    Authors = fullLoan.Copy.Variant.Volume.Book.Authors.Select(a => a.AuthorName).ToList()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error creating loan: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ReturnLoanAsync(int loanId)
        {
            var loan = await _context.Loans
                .Include(l => l.Copy)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null || loan.LoanStatus == "Returned")
                return false;

            loan.LoanStatus = "Returned";
            loan.ReturnDate = DateTime.Now;

            if (loan.Copy != null)
                {
                loan.Copy.CopyStatus = "Available";

                // FIX 4: Get variant ID correctly
                var variantId = loan.Copy.VariantId;

                // Check if there are pending reservations
                var hasPendingReservations = await _context.Reservations
                    .AnyAsync(r => r.VariantId == variantId && r.ReservationStatus == "Pending");

                if (hasPendingReservations)
                    {
                    await _reservationService.NotifyNextReservationAsync(variantId);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Loan>> GetUserLoansAsync(int userId)
        {
            return await _context.Loans
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.BorrowDate)
                .ToListAsync();
        }

        public async Task<BookInfoDTO?> GetBookByBarcodeAsync(string barcode)
        {
            var copy = await _context.BookCopies
                .Include(c => c.Variant)
                    .ThenInclude(v => v.Volume)
                        .ThenInclude(vol => vol.Book)
                            .ThenInclude(b => b.Authors)
                .Include(c => c.Variant.Publisher)
                .Include(c => c.Variant.Edition)
                .Include(c => c.Variant.CoverType)
                .Include(c => c.Variant.PaperQuality)
                .Where(c => c.Barcode == barcode)
                .Select(c => new BookInfoDTO
                {
                    CopyId = c.CopyId,
                    Barcode = c.Barcode,
                    CopyStatus = c.CopyStatus,
                    Location = c.Location,

                    VariantId = c.VariantId,
                    VolumeId = c.Variant.VolumeId,
                    BookId = c.Variant.Volume.BookId,

                    Title = c.Variant.Volume.Book.Title,
                    VolumeTitle = c.Variant.Volume.VolumeTitle ?? "",
                    Authors = c.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),

                    PublisherName = c.Variant.Publisher.PublisherName,
                    EditionName = c.Variant.Edition != null ? c.Variant.Edition.EditionName : "",
                    PublicationYear = c.Variant.PublicationYear,
                    CoverTypeName = c.Variant.CoverType != null ? c.Variant.CoverType.CoverTypeName : "",
                    PaperQualityName = c.Variant.PaperQuality != null ? c.Variant.PaperQuality.PaperQualityName : "",
                    Price = c.Variant.Price,
                    ISBN = c.Variant.Isbn ?? "",
                    Notes = c.Variant.Notes ?? ""
                })
                .FirstOrDefaultAsync();

            return copy;
        }

        public async Task<UserInfoDTO?> GetUserByQueryAsync(string query)
        {
            return await _context.Users
                .Where(u => u.Username == query || u.Email == query || u.Phone == query)
                .Select(u => new UserInfoDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone ?? ""
                })
                .FirstOrDefaultAsync();
        }

        public IQueryable<LoanListDTO> GetAllLoans()
        {
            // FIX 5: Thread-safe penalty update
            lock (_penaltyLock)
            {
                if (LastPenaltyUpdateDate.Date < DateTime.Today)
                {
                    UpdatePenaltiesAndStatuses();
                    LastPenaltyUpdateDate = DateTime.Today;
                }
            }

            return _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Variant)
                        .ThenInclude(v => v.Volume)
                            .ThenInclude(vol => vol.Book)
                                .ThenInclude(ba => ba.Authors)
                .Select(l => new LoanListDTO
                {
                    LoanId = l.LoanId,
                    BorrowDate = l.BorrowDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    LoanStatus = l.LoanStatus,
                    FineAmount = l.FineAmount ?? 0,
                    Extended = l.Extended ?? false,

                    Username = l.User.Username,
                    FullName = l.User.FullName,
                    Email = l.User.Email,

                    Barcode = l.Copy.Barcode,
                    Title = l.Copy.Variant.Volume.Book.Title,
                    VolumeTitle = l.Copy.Variant.Volume.VolumeTitle ?? "",
                    Authors = l.Copy.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList()
                });
        }

        private void UpdatePenaltiesAndStatuses()
        {
            try
            {
                var toBeOverdue = _context.Loans
                    .Where(l => l.LoanStatus == "Borrowed" && l.DueDate < DateTime.Today)
                    .ToList();

                foreach (var loan in toBeOverdue)
                {
                    loan.LoanStatus = "Overdue";
                }

                _context.SaveChanges();

                var overdueLoans = _context.Loans
                    .Include(l => l.Copy)
                        .ThenInclude(c => c.Variant)
                    .Where(l => l.LoanStatus == "Overdue")
                    .ToList();

                foreach (var loan in overdueLoans)
                {
                    int daysLate = (DateTime.Today - loan.DueDate.Date).Days;
                    decimal fine = daysLate * 5000;

                    decimal maxPrice = loan.Copy?.Variant?.Price ?? 0;
                    if (fine > maxPrice && maxPrice > 0)
                    {
                        loan.FineAmount = maxPrice;
                    }
                    else
                    {
                        loan.FineAmount = fine;
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                Console.WriteLine($"Error updating penalties: {ex.Message}");
            }
        }

        public async Task<int> GetLoansCountAsync(string? keyword)
        {
            var query = _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Variant)
                        .ThenInclude(v => v.Volume)
                            .ThenInclude(vol => vol.Book).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(l =>
                    l.Copy.Variant.Volume.Book.Title.Contains(keyword) ||
                    l.User.FullName.Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<bool> ExtendLoanAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null || loan.LoanStatus == "Returned")
                return false;

            // Check if already extended
            if (loan.Extended == true)
                return false;

            loan.DueDate = loan.DueDate.AddDays(7);
            loan.Extended = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PayFineAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return false;

            loan.FineAmount = 0;

            // If loan is overdue but fine is paid, keep status as Overdue
            // Don't change to Borrowed unless book is returned

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LoanListDTO?> GetLoanByIdAsync(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Variant)
                        .ThenInclude(v => v.Volume)
                            .ThenInclude(vol => vol.Book)
                                .ThenInclude(ba => ba.Authors)
                .Where(l => l.LoanId == id)
                .Select(l => new LoanListDTO
                {
                    LoanId = l.LoanId,
                    BorrowDate = l.BorrowDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    LoanStatus = l.LoanStatus,
                    FineAmount = l.FineAmount ?? 0,
                    Extended = l.Extended ?? false,

                    Username = l.User.Username,
                    FullName = l.User.FullName,
                    Email = l.User.Email,

                    Barcode = l.Copy.Barcode,
                    Title = l.Copy.Variant.Volume.Book.Title,
                    VolumeTitle = l.Copy.Variant.Volume.VolumeTitle ?? "",
                    Authors = l.Copy.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList()
                })
                .FirstOrDefaultAsync();

            return loan;
        }

        public async Task<bool> UpdateLoanAsync(int loanId, LoanEditDTO dto)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null) return false;

            loan.LoanStatus = dto.LoanStatus;
            loan.FineAmount = dto.FineAmount;
            loan.Extended = dto.Extended;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanUserBorrowDirectlyAsync(int userId, int variantId)
        {
            var pendingReservations = await _context.Reservations
                .Where(r => r.VariantId == variantId && r.ReservationStatus == "Pending")
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            if (!pendingReservations.Any())
                return true;

            var firstReservation = pendingReservations.First();
            return firstReservation.UserId == userId;
        }

        public async Task<List<BookCopyDTO>> GetAvailableCopiesAsync(int variantId)
        {
            return await _context.BookCopies
                .Where(c => c.VariantId == variantId && c.CopyStatus == "Available")
                .Select(c => new BookCopyDTO
                {
                    CopyId = c.CopyId,
                    Barcode = c.Barcode,
                    Location = c.Location,
                    CopyStatus = c.CopyStatus,
                    VariantId = c.VariantId
                })
                .ToListAsync();
        }

        public async Task UpdateCopyStatusAsync(int copyId, string status)
        {
            var copy = await _context.BookCopies.FindAsync(copyId);
            if (copy != null)
            {
                copy.CopyStatus = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SendDueDateRemindersAsync()
        {
            var today = DateTime.Now.Date;
            var startOfDay = today;
            var endOfDay = today.AddDays(1);

            Console.WriteLine("📌 [Reminder] Bắt đầu gửi DueDateReminder...");

            var dueSoonLoans = await _context.Loans
                .Include(l => l.User)
                .Where(l => l.LoanStatus == "Borrowed"
                    && l.DueDate.Date >= today
                    && l.DueDate.Date <= today.AddDays(2))
                .ToListAsync();

            Console.WriteLine($"📊 [Reminder] Có {dueSoonLoans.Count} khoản mượn sắp đến hạn.");

            var userDueLoans = dueSoonLoans
                .GroupBy(l => l.UserId)
                .ToList();

            foreach (var group in userDueLoans)
            {
                var user = group.First().User;
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"⚠️ [Reminder] User {group.Key} thiếu thông tin hoặc email.");
                    continue;
                }

                Console.WriteLine($"🔔 [Reminder] Đang xử lý User {user.UserId} ({user.Email})...");

                var alreadySent = await _context.Notifications.AnyAsync(n =>
                    n.ReceiverId == user.UserId &&
                    n.NotificationType == "DueDateReminder" &&
                    n.NotificationDate >= startOfDay && n.NotificationDate < endOfDay
                );

                if (alreadySent)
                {
                    Console.WriteLine($"✅ [Reminder] User {user.UserId}: Đã gửi hôm nay, bỏ qua.");
                    continue;
                }

                var loanList = string.Join("", group.Select(l => $"- Loan ID: {l.LoanId}, Due on: {l.DueDate:yyyy-MM-dd}\n"));

                var plainBody = $@"Dear {user.FullName ?? "User"},

        The following loans are due soon:
        {loanList}
        Please return the books on time to avoid fines.

        Thank you.";

                var htmlLoanList = string.Join("", group.Select(l => $"<li>Loan ID: {l.LoanId}, Due on: {l.DueDate:yyyy-MM-dd}</li>"));

                var trackingUrl = "http://localhost:5027/api/notifications/track?notificationId=0";

                var htmlBodyTemplate = $@"<p>Dear {user.FullName ?? "User"},</p>
        <p>The following loans are due soon:</p>
        <ul>{htmlLoanList}</ul>
        <p>Please return the books on time to avoid fines.</p>
        <p>Please <a href='{trackingUrl}'>click here</a> to confirm reading this email.</p>
        <p>Thank you.</p>";

                try
                {
                    Console.WriteLine($"📤 [Reminder] Gửi email đến {user.Email}...");
                    await _emailService.SendMailAsync(user.Email, "Library Due Date Reminder", htmlBodyTemplate);
                    Console.WriteLine("✅ [Reminder] Email đã gửi thành công.");

                    var notification = new Notification
                    {
                        SenderId = null,
                        SenderType = "System",
                        ReceiverId = user.UserId,
                        ForStaff = false,
                        Message = plainBody,
                        NotificationDate = DateTime.Now,
                        NotificationType = "DueDateReminder",
                        ReadStatus = false
                    };

                    Console.WriteLine("💾 [Reminder] Đang lưu notification vào database...");
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ [Reminder] Đã lưu notification cho User {user.UserId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [Reminder] Lỗi khi gửi mail tới {user.Email}: {ex.Message}");
                }
            }
        }

        public async Task SendFineNotificationsAsync()
        {
            var today = DateTime.Now.Date;
            var startOfDay = today;
            var endOfDay = today.AddDays(1);

            var overdueLoansWithFines = await _context.Loans
                .Include(l => l.User)
                .Where(l => l.LoanStatus == "Overdue" && l.FineAmount > 0)
                .ToListAsync();

            foreach (var loan in overdueLoansWithFines)
            {
                var user = loan.User;
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"Loan {loan.LoanId}: Missing user or email.");
                    continue;
                }

                var alreadySent = await _context.Notifications.AnyAsync(n =>
                    n.ReceiverId == user.UserId &&
                    n.NotificationType == "FineNotice" &&
                    n.RelatedId == loan.LoanId &&
                    n.NotificationDate >= startOfDay && n.NotificationDate < endOfDay
                );

                if (alreadySent)
                {
                    Console.WriteLine($"User {user.UserId}, Loan {loan.LoanId}: Fine notice already sent today.");
                    continue;
                }

                var body = $@"Dear {user.FullName ?? "User"},

        You have an outstanding fine of {loan.FineAmount:C} for an overdue loan (Loan ID: {loan.LoanId}).

        Please pay the fine at your earliest convenience to avoid further action.

        Thank you.";

                try
                {
                    await _emailService.SendMailAsync(user.Email, "Library Fine Notice", body);

                    _context.Notifications.Add(new Notification
                    {
                        SenderId = null,
                        SenderType = "System",
                        ReceiverId = user.UserId,
                        ForStaff = false,
                        Message = body,
                        NotificationDate = DateTime.Now,
                        NotificationType = "FineNotice",
                        RelatedTable = "loans",
                        RelatedId = loan.LoanId,
                        ReadStatus = false
                    });

                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Fine notice sent and saved for User {user.UserId}, Loan {loan.LoanId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending fine notice to {user.Email}: {ex.Message}");
                }
            }
        }

        public async Task UpdateOverdueLoansAndFinesAsync()
        {
            var now = DateTime.Now.Date;

            var overdueLoans = await _context.Loans
                .Where(l => l.LoanStatus == "Borrowed" && l.DueDate < now)
                .ToListAsync();

            foreach (var loan in overdueLoans)
            {
                var daysOverdue = (now - loan.DueDate.Date).Days;
                if (daysOverdue <= 0) continue;

                loan.LoanStatus = "Overdue";
                decimal fine = 5000 * daysOverdue;
                loan.FineAmount = fine;

                Console.WriteLine($"Loan {loan.LoanId}: Overdue {daysOverdue} day(s), fine = {fine}.");
            }

            await _context.SaveChangesAsync();
        }

        //the
        public async Task<int> CountTotalLoansAsync()
        {
            return await _context.Loans.CountAsync();
        }

        public async Task<int> CountOverdueLoansAsync()
        {
            return await _context.Loans.CountAsync(l => l.LoanStatus == "Overdue");
        }

        public async Task<decimal?> GetTotalFineAmountAsync()
        {
            return await _context.Loans.SumAsync(l => l.FineAmount);
        }

        public async Task<List<MonthlyStatDto>> GetLoansPerMonthAsync()
        {
            var result = _context.Loans
                .AsEnumerable() // chuyển sang xử lý in-memory
                .GroupBy(l => l.BorrowDate.ToString("yyyy-MM"))
                .Select(g => new MonthlyStatDto
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return await Task.FromResult(result); // vẫn giữ async để đồng bộ interface
        }

        public async Task<List<BookHomepageDto>> GetPopularHomepageBooksAsync(int top = 5)
        {
            // Lấy top bookId được mượn nhiều nhất
            var topBookIdsWithCount = await (
                from l in _context.Loans
                join c in _context.BookCopies on l.CopyId equals c.CopyId
                join v in _context.BookVariants on c.VariantId equals v.VariantId
                join vol in _context.BookVolumes on v.VolumeId equals vol.VolumeId
                group l by vol.BookId into g
                orderby g.Count() descending
                select new
                {
                    BookId = g.Key,
                    BorrowCount = g.Count()
                }
            ).Take(top).ToListAsync();

            var topBookIds = topBookIdsWithCount.Select(x => x.BookId).ToList();

            // Truy vấn danh sách sách cùng với Authors và Category
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Category)
                .Include(b => b.BookVolumes)
                    .ThenInclude(vol => vol.BookVariants)
                        .ThenInclude(variant => variant.BookCopies)
                .Where(b => topBookIds.Contains(b.BookId))
                .ToListAsync();

            var bookDtos = books.Select(b => new BookHomepageDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Category = b.Category?.CategoryName ?? "Không rõ",
                Language = b.Language ?? "Không rõ",
                Image = b.CoverImg ?? "",
                Authors = b.Authors.Select(a => a.AuthorName).ToList(),
                Available = b.BookVolumes
                    .SelectMany(v => v.BookVariants)
                    .SelectMany(v => v.BookCopies)
                    .Any(c => c.CopyStatus == "Available")
            }).ToList();

            // Sắp xếp lại đúng theo borrow count
            var sortedDtos = bookDtos
                .Join(topBookIdsWithCount,
                      dto => dto.BookId,
                      stat => stat.BookId,
                      (dto, stat) => new { Book = dto, Count = stat.BorrowCount })
                .OrderByDescending(x => x.Count)
                .Select(x => x.Book)
                .ToList();

            return sortedDtos;
        }
    }
}

