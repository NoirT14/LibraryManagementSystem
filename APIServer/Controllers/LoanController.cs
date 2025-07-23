using APIServer.DTO.Loan;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost("send-due-reminders")]
        public async Task<IActionResult> SendDueReminders()
        {
            await _loanService.SendDueDateRemindersAsync();
            return Ok("Due date reminders sent");
        }

       

    }
}
