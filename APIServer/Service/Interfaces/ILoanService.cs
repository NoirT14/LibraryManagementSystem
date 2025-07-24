using APIServer.DTO.Loan;
using APIServer.DTO.Loans;
using APIServer.Models;

namespace APIServer.Service.Interfaces
{
    public interface ILoanService
    {//
        Task<LoanListDTO> CreateLoanAsync(LoanCreateDTO dto);
        Task<bool> ReturnLoanAsync(int loanId);
        Task<IEnumerable<Loan>> GetUserLoansAsync(int userId);
        Task<BookInfoDTO?> GetBookByBarcodeAsync(string barcode);
        Task<UserInfoDTO?> GetUserByQueryAsync(string query);
        IQueryable<LoanListDTO> GetAllLoans();
        Task<int> GetLoansCountAsync(string? keyword);
        Task<bool> ExtendLoanAsync(int loanId);
        Task<bool> PayFineAsync(int loanId);
        Task<LoanListDTO?> GetLoanByIdAsync(int id);
        Task<bool> UpdateLoanAsync(int loanId, LoanEditDTO dto);
        Task<bool> CanUserBorrowDirectlyAsync(int userId, int variantId);
        Task<List<BookCopyDTO>> GetAvailableCopiesAsync(int variantId);
        Task UpdateCopyStatusAsync(int copyId, string status);
       
        Task SendDueDateRemindersAsync();
        Task SendFineNotificationsAsync();
        Task UpdateOverdueLoansAndFinesAsync();

        //the
        Task<int> CountTotalLoansAsync();
        Task<int> CountOverdueLoansAsync();
        Task<decimal?> GetTotalFineAmountAsync();

        Task<List<MonthlyStatDto>> GetLoansPerMonthAsync();


    }
}
