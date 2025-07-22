using APIServer.DTO.Loan;
using APIServer.Models;

namespace APIServer.Service.Interfaces
{
    public interface ILoanService
    {
       
        Task SendDueDateRemindersAsync();
        Task SendFineNotificationsAsync();
        Task UpdateOverdueLoansAndFinesAsync();

        Task<List<LoanWithVolumeDto>> GetLoansWithVolumeByUserIdAsync(int userId);

    }
}
