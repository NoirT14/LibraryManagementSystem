using APIServer.DTO.Book;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using APIServer.Services;

namespace APIServer.Controllers.OData
{
    [Route("odata/[controller]")]
    public class BooksController : ODataController
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDetailDTO>> GetBookDetail(int id)
        {
            var book = await _bookService.GetBookDetailByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { error = "Không tìm thấy sách." });
            }

            return Ok(book);
        }
    }
    

}
