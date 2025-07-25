using APIServer.DTO.Reservations;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // USER ENDPOINTS.

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var reservation = await _reservationService.CreateReservationAsync(dto);

                if (reservation == null)
                    return BadRequest(new
                    {
                        message = "Không thể tạo đặt trước. Người dùng có thể đang nợ phạt, đã có đặt trước cho sách này, hoặc sách đang có sẵn."
                    });

                return CreatedAtAction(nameof(GetReservationById),
                    new { id = reservation.ReservationId },
                    reservation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo đặt trước" });
            }
        }

        [HttpDelete("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(int id, [FromQuery] int userId)
        {
            try
            {
                var success = await _reservationService.CancelReservationAsync(id, userId);

                if (!success)
                    return NotFound(new { message = "Không tìm thấy đặt trước hoặc không thể hủy" });

                return Ok(new { message = "Hủy đặt trước thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi hủy đặt trước" });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserReservations(int userId)
        {
            try
            {
                var reservations = await _reservationService.GetUserReservationsAsync(userId);
                return Ok(reservations);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách đặt trước" });
            }
        }

        [HttpGet("availability/{variantId}")]
        [Authorize]
        public async Task<IActionResult> GetBookAvailability(int variantId)
        {
            try
            {
                var availability = await _reservationService.GetBookAvailabilityAsync(variantId);

                if (availability == null)
                    return NotFound(new { message = "Không tìm thấy thông tin sách" });

                return Ok(availability);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra tình trạng sách" });
            }
        }

        [HttpGet("availability/book/{bookId}")]
        public async Task<IActionResult> GetBookAvailabilityByBookId(int bookId)
        {
            try
            {
                var availabilities = await _reservationService.GetBookAvailabilityByBookIdAsync(bookId);

                if (availabilities == null || !availabilities.Any())
                    return NotFound(new { message = "Không tìm thấy sách có thể đặt trước" });

                return Ok(availabilities);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra tình trạng sách" });
            }
        }

        // STAFF ENDPOINTS

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAllReservations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? status = null)
        {
            try
            {
                // Validate pagination
                page = Math.Max(1, page);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var reservations = await _reservationService.GetAllReservationsListAsync(
                    page, pageSize, keyword, status);

                var totalCount = await _reservationService.GetReservationsCountAsync(keyword);

                return Ok(new
                {
                    data = reservations,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách đặt trước" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReservationById(int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(id);

                if (reservation == null)
                    return NotFound(new { message = "Không tìm thấy đặt trước" });

                return Ok(reservation);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationUpdateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var success = await _reservationService.UpdateReservationAsync(id, dto);

                if (!success)
                    return NotFound(new { message = "Không tìm thấy đặt trước" });

                return Ok(new { message = "Cập nhật đặt trước thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}/staff-cancel")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CancelReservationByStaff(int id, [FromQuery] int staffId)
        {
            try
            {
                if (staffId <= 0)
                    return BadRequest(new { message = "Staff ID không hợp lệ" });

                var success = await _reservationService.CancelReservationByStaffAsync(id, staffId);

                if (!success)
                    return NotFound(new { message = "Không tìm thấy đặt trước hoặc không thể hủy" });

                return Ok(new { message = "Nhân viên đã hủy đặt trước thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi hủy đặt trước" });
            }
        }

        [HttpGet("queue/{variantId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetReservationQueue(int variantId)
        {
            try
            {
                var queue = await _reservationService.GetReservationQueueAsync(variantId);
                return Ok(queue);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách chờ" });
            }
        }

        [HttpGet("{id}/position")]
        [Authorize]
        public async Task<IActionResult> GetQueuePosition(int id)
        {
            try
            {
                var position = await _reservationService.GetQueuePositionAsync(id);

                if (position == 0)
                    return NotFound(new { message = "Không tìm thấy đặt trước" });

                return Ok(new { reservationId = id, queuePosition = position });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi" });
            }
        }

        [HttpGet("next/{variantId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetNextAvailableReservation(int variantId)
        {
            try
            {
                var reservation = await _reservationService.GetNextAvailableReservationAsync(variantId);

                if (reservation == null)
                    return NotFound(new { message = "Không có đặt trước nào đang chờ cho sách này" });

                return Ok(reservation);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi" });
            }
        }

        [HttpPost("process-expired")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ProcessExpiredReservations()
        {
            try
            {
                await _reservationService.ProcessExpiredReservationsAsync();
                return Ok(new { message = "Xử lý đặt trước hết hạn thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý đặt trước hết hạn" });
            }
        }
    }
}