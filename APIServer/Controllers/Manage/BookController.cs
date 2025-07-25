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
        public async Task<IActionResult> Post([FromBody] BookInfoRequest book)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _bookService.Create(book);
            return Created(created);
        }

        [HttpPatch("({key})")]
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<Book> delta)
        {
            var updated = await _bookService.Update(key, delta);
            if (updated == null) return NotFound();
            return Updated(updated);
        }

        [HttpDelete("({key})")]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var deleted = await _bookService.Delete(key);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
