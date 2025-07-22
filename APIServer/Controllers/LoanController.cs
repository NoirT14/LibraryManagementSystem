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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<LoanWithVolumeDto>>> GetLoansByUserId(int userId)
        {
            var loans = await _loanService.GetLoansWithVolumeByUserIdAsync(userId);

            if (loans == null || loans.Count == 0)
            {
                return NotFound($"No loans found for user with ID {userId}");
            }

            return Ok(loans);
        }

    }
}
