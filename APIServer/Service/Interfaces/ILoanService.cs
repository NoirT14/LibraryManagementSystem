using APIServer.DTO.Loans;
using APIServer.Models;

namespace APIServer.Service.Interfaces
{
    public interface ILoanService
    {
        Task<Loan> CreateLoanAsync(LoanCreateDTO dto);
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
    }
}
