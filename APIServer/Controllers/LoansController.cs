using APIServer.DTO.Loans;
using APIServer.Service.Interfaces;
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

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanCreateDTO dto)
        {
            var loan = await _loanService.CreateLoanAsync(dto);
            if (loan == null)
            {
                return BadRequest("Người mượn đang nợ tiền phạt hoặc có sách quá hạn chưa trả, không thể mượn thêm.");
            }
            return Ok(loan);
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            var success = await _loanService.ReturnLoanAsync(id);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLoans(int userId)
        {
            var loans = await _loanService.GetUserLoansAsync(userId);
            return Ok(loans);
        }

        [HttpGet("book")]
        public async Task<IActionResult> GetBookByBarcode([FromQuery] string barcode)
        {
            var book = await _loanService.GetBookByBarcodeAsync(barcode);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserByQuery([FromQuery] string query)
        {
            var user = await _loanService.GetUserByQueryAsync(query);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [EnableQuery]
        [HttpGet]
        public IQueryable<LoanListDTO> GetAllLoans()
        {
            return _loanService.GetAllLoans();
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetLoansCount([FromQuery] string? keyword)
        {
            var count = await _loanService.GetLoansCountAsync(keyword);
            return Ok(count);
        }

        [HttpPut("{id}/extend")]
        public async Task<IActionResult> ExtendLoan(int id)
        {
            var success = await _loanService.ExtendLoanAsync(id);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPut("{id}/payfine")]
        public async Task<IActionResult> PayFine(int id)
        {
            var success = await _loanService.PayFineAsync(id);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            if (loan == null)
                return NotFound();
            return Ok(loan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanEditDTO dto)
        {
            var updated = await _loanService.UpdateLoanAsync(id, dto);
            if (!updated)
                return NotFound();
            return Ok();
        }


    }
}
