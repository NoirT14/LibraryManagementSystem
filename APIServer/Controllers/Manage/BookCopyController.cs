using APIServer.DTO.Book;
using APIServer.DTO.Loans;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers.Manage
{
    [Route("api/manage/[controller]")]
    [ApiController]
    public class BookCopyController : Controller
    {
        private readonly IBookCopyService _bookCopyService;

        public BookCopyController(IBookCopyService bookCopyService)
        {
            _bookCopyService = bookCopyService;
        }

        [HttpGet]
        public IActionResult Get(ODataQueryOptions<BookCopyInfoListDto> options)
        {
            var result = _bookCopyService.GetFilteredBookCopies(options);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookCopyFull([FromBody] BookCopyRequest request)
        {
            var result = await _bookCopyService.CreateBookCopyFullAsync(request);

            if (result == null)
                return NotFound(new { message = "Book volume not found." });

            return Created("", result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCopyAndVariantRequest request)
        {
            var result = await _bookCopyService.UpdateCopyAndVariantAsync(id, request);

            if (!result)
                return NotFound(new { message = "Book copy not found." });

            return Ok(new { message = "Update successful." });
        }
    }
}
