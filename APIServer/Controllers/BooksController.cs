using APIServer.DTO.Book;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using APIServer.Service;

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

        [HttpGet("books")]
        public async Task<IActionResult> GetBooksForHomepage()
        {
            var books = await _bookService.GetBooksForHomepageAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookDetail(int id)
        {
            var result = await _bookService.GetBookDetailAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }


    }


}