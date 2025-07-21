using APIServer.Data;
using APIServer.DTO.Loans;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class LoanService : ILoanService
    {
        private readonly LibraryDatabaseContext _context;
        private static DateTime LastPenaltyUpdateDate = DateTime.MinValue;

        public LoanService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Loan?> CreateLoanAsync(LoanCreateDTO dto)
        {
            var hasUnpaidFines = await _context.Loans
                .Where(l => l.UserId == dto.UserId &&
                           (l.LoanStatus == "Overdue" || (l.FineAmount > 0 && l.LoanStatus != "Returned")))
                .AnyAsync();

            if (hasUnpaidFines)
            {
                return null;
            }

            var loan = new Loan
            {
                UserId = dto.UserId,
                CopyId = dto.CopyId,
                BorrowDate = DateTime.Now,
                DueDate = dto.DueDate,
                LoanStatus = "Borrowed",
                ReservationId = dto.ReservationId
            };

            _context.Loans.Add(loan);

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
            return loan;
        }

        public async Task<bool> ReturnLoanAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null || loan.LoanStatus == "Returned")
                return false;

            loan.LoanStatus = "Returned";
            loan.ReturnDate = DateTime.Now;

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
                    VolumeTitle = c.Variant.Volume.VolumeTitle,
                    Authors = c.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList(),

                    PublisherName = c.Variant.Publisher.PublisherName,
                    EditionName = c.Variant.Edition.EditionName,
                    PublicationYear = c.Variant.PublicationYear,
                    CoverTypeName = c.Variant.CoverType.CoverTypeName,
                    PaperQualityName = c.Variant.PaperQuality.PaperQualityName,
                    Price = c.Variant.Price,
                    ISBN = c.Variant.Isbn,
                    Notes = c.Variant.Notes
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
                    Phone = u.Phone
                })
                .FirstOrDefaultAsync();
        }

        public IQueryable<LoanListDTO> GetAllLoans()
        {
            if (LastPenaltyUpdateDate.Date < DateTime.Today)
            {
                UpdatePenaltiesAndStatuses();
                LastPenaltyUpdateDate = DateTime.Today;
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
                    FineAmount = (decimal)l.FineAmount,
                    Extended = (bool)l.Extended,

                    Username = l.User.Username,
                    FullName = l.User.FullName,
                    Email = l.User.Email,

                    Barcode = l.Copy.Barcode,
                    Title = l.Copy.Variant.Volume.Book.Title,
                    VolumeTitle = l.Copy.Variant.Volume.VolumeTitle,
                    Authors = l.Copy.Variant.Volume.Book.Authors.Select(ba => ba.AuthorName).ToList()
                });
        }

        private void UpdatePenaltiesAndStatuses()
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
                if (fine > maxPrice)
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
            if (loan == null) return false;
            if (loan.Extended == null) loan.Extended = false;
            if ((bool)loan.Extended) return false;
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
                    FineAmount = (decimal)l.FineAmount,
                    Extended = (bool)l.Extended,

                    Username = l.User.Username,
                    FullName = l.User.FullName,
                    Email = l.User.Email,

                    Barcode = l.Copy.Barcode,
                    Title = l.Copy.Variant.Volume.Book.Title,
                    VolumeTitle = l.Copy.Variant.Volume.VolumeTitle,
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

    }
}
