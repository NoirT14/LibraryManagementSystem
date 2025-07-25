using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using APIServer.Service.Interfaces; // đổi theo namespace thật
using APIServer.DTO;              // nếu có DTO như MonthlyStatDto

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IUserService _userService;
        private readonly IReservationService _reservationService;
        private readonly IBookService _bookService;

        public DashboardController(
            ILoanService loanService,
            IUserService userService,
            IReservationService reservationService,
            IBookService bookService
            )
        {
            _loanService = loanService;
            _userService = userService;
            _reservationService = reservationService;
            _bookService = bookService;
            
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = new
            {
                // 🕮 Mượn trả
                TotalLoans = await _loanService.CountTotalLoansAsync(),
                OverdueLoans = await _loanService.CountOverdueLoansAsync(),
                TotalFines = await _loanService.GetTotalFineAmountAsync(),

                // 📘 Đặt giữ
                TotalReservations = await _reservationService.CountReservationsAsync(),
                AvailableReservations = await _reservationService.CountByStatusAsync("Available"),

                // 📚 Sách theo danh mục
                BooksByCategory = await _bookService.GetBookCountByCategoryAsync()
            };

            return Ok(summary);
        }

        [HttpGet("monthly-loans")]
        public async Task<IActionResult> GetLoansPerMonth()
        {
            var data = await _loanService.GetLoansPerMonthAsync();
            return Ok(data);
        }
    }
}
