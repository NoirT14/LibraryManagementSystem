namespace APIServer.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string toEmail, string subject, string body);
    }
}
