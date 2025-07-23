using APIServer.DTO.Loan;

namespace APIServer.Service.Interfaces
{
    public interface ILoanService
    {
       
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
