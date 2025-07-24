using APIServer.DTO.Loans;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanCreateDTO dto)
        {
            try
            {
                // Basic validation.
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var loanDto = await _loanService.CreateLoanAsync(dto);
                if (loanDto == null)
                {
                    return BadRequest(new { message = "Không thể tạo phiếu mượn. Người mượn có thể đang nợ tiền phạt hoặc sách không khả dụng." });
                }

                return Ok(loanDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo phiếu mượn" });
            }
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            try
            {
                var success = await _loanService.ReturnLoanAsync(id);
                if (!success)
                    return NotFound(new { message = "Không tìm thấy phiếu mượn hoặc sách đã được trả" });

                return Ok(new { message = "Trả sách thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi trả sách" });
            }
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLoans(int userId)
        {
            try
            {
                var loans = await _loanService.GetUserLoansAsync(userId);
                return Ok(loans);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách phiếu mượn" });
            }
        }

        [Authorize]
        [HttpGet("book")]
        public async Task<IActionResult> GetBookByBarcode([FromQuery] string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode))
                    return BadRequest(new { message = "Barcode không được để trống" });

                var book = await _loanService.GetBookByBarcodeAsync(barcode);
                if (book == null)
                    return NotFound(new { message = "Không tìm thấy sách với barcode này" });

                return Ok(book);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm sách" });
            }
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserByQuery([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                    return BadRequest(new { message = "Query không được để trống" });

                var user = await _loanService.GetUserByQueryAsync(query);
                if (user == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm người dùng" });
            }
        }

        [Authorize]
        [EnableQuery]
        [HttpGet]
        public IQueryable<LoanListDTO> GetAllLoans()
        {
            // OData handles exceptions internally
            return _loanService.GetAllLoans();
        }

        [Authorize]
        [HttpGet("count")]
        public async Task<IActionResult> GetLoansCount([FromQuery] string? keyword)
        {
            try
            {
                var count = await _loanService.GetLoansCountAsync(keyword);
                return Ok(new { count });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi" });
            }
        }

        [Authorize]
        [HttpPut("{id}/extend")]
        public async Task<IActionResult> ExtendLoan(int id)
        {
            try
            {
                var success = await _loanService.ExtendLoanAsync(id);
                if (!success)
                    return BadRequest(new { message = "Không thể gia hạn. Phiếu mượn không tồn tại hoặc đã được gia hạn trước đó." });

                return Ok(new { message = "Gia hạn thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi gia hạn" });
            }
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{id}/payfine")]
        public async Task<IActionResult> PayFine(int id)
        {
            try
            {
                var success = await _loanService.PayFineAsync(id);
                if (!success)
                    return NotFound(new { message = "Không tìm thấy phiếu mượn" });

                return Ok(new { message = "Thanh toán tiền phạt thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi thanh toán" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            try
            {
                var loan = await _loanService.GetLoanByIdAsync(id);
                if (loan == null)
                    return NotFound(new { message = "Không tìm thấy phiếu mượn" });

                return Ok(loan);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi" });
            }
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanEditDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updated = await _loanService.UpdateLoanAsync(id, dto);
                if (!updated)
                    return NotFound(new { message = "Không tìm thấy phiếu mượn" });

                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật" });
            }
        }
    }
}