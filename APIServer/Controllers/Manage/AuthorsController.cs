using APIServer.DTO.Author;
using APIServer.Service;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Controllers.Manage
{
    [Route("api/[controller]")]
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _service;

        public AuthorsController(IAuthorService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<AuthorRespone> GetAll()
        {
            return _service.GetAllAsQueryable();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] AuthorRequest request)
        {
            var result = await _service.UpdateAuthorAsync(id, request);
            if (result == null)
                return NotFound(new { message = "Author not found" });

            return Ok(new { message = "Updated successfully", data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AuthorRequest request)
        {
            var createdAuthor = await _service.CreateAuthorAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdAuthor.AuthorId }, new
            {
                message = "Author created successfully",
                data = createdAuthor
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var author = await _service.GetByIdAsync(id);
            if (author == null)
                return NotFound(new { message = "Author not found" });

            return Ok(new
            {
                message = "Author retrieved successfully",
                data = author
            });
        }
    }
}
