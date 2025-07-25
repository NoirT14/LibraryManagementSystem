using APIServer.DTO.Author;
using APIServer.DTO.Category;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers.Manage
{
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<CategoryResponse> GetAll()
        {
            return _service.GetAllAsQueryable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CategoryRequest dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
