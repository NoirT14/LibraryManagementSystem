namespace APIServer.Service.Interfaces
{
    public interface ILoanService
    {
        Task BorrowBookAsync(int userId, int copyId);
        Task ReturnBookAsync(int loanId);
        Task SendDueDateRemindersAsync();

    }
}
