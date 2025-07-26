using APIServer.DTO.Reservation;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// Đặt giữ sách
        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromQuery] int userId, [FromQuery] int volumeId)
        {
            try
            {
                await _reservationService.CreateReservationAsync(userId, volumeId);
                return Ok(new { message = "Đặt giữ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Lỗi đặt giữ: " + ex.Message });
            }
        }

        /// Kiểm tra bản sao có sẵn và cập nhật trạng thái đặt giữ (chạy định kỳ bởi hệ thống hoặc staff)
        [HttpPost("check-available")]
        public async Task<IActionResult> CheckAvailableReservations()
        {
            await _reservationService.CheckAvailableReservationsAsync();
            return Ok(new { message = "Đã kiểm tra và cập nhật đặt giữ." });
        }

        /// Hủy các đơn đặt giữ đã hết hạn
        [HttpPost("expire-old")]
        public async Task<IActionResult> ExpireOldReservations()
        {
            await _reservationService.ExpireOldReservationsAsync();
            return Ok(new { message = "Đã xử lý các đặt giữ hết hạn." });
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ReservationDto>>> GetReservationsByUser(int userId)
        {
            var reservations = await _reservationService.GetReservationsByUserAsync(userId);
            return Ok(reservations);
        }
    }
}
