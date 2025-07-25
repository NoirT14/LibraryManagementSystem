using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using APIServer.Service;
using APIServer.DTO.Book;

namespace APIServer.Controllers.Manage
{
    [Route("api/manage/[controller]")]
    public class BookController : ODataController
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            var raw = _bookService.GetAll();

            var data = raw.ToList().Select(BookService.ToDto);
            return Ok(data);
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> Get(int key)
        {
            var bookDetail = await _bookService.GetBookDetailById(key);
            if (bookDetail == null) return NotFound();
            return Ok(bookDetail);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] BookInfoRequest book)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _bookService.Create(book);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] BookInfoRequest book)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _bookService.Update(id, book);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _bookService.Delete(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetBookAllFieldDetail(int id)
        {
            var result = await _bookService.GetBookAllFieldAsync(id);

            if (result == null)
                return NotFound(new { message = "Book not found." });

            return Ok(result);
        }

        [HttpGet("getBookForCopy")]
        [EnableQuery]
        public IActionResult GetBookForCopy(ODataQueryOptions<BookInfoListSearchCopyRespone> options)
        {
            var result = _bookService.GetBookInfoList(options);
            return Ok(result);
        }
    }
}
