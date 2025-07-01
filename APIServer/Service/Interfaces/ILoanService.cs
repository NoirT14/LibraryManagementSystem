namespace APIServer.Service.Interfaces
{
    public interface ILoanService
    {
       
        Task SendDueDateRemindersAsync();
        Task SendFineNotificationsAsync();
        Task UpdateOverdueLoansAndFinesAsync();

    }
}
