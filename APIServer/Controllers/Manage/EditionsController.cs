using APIServer.DTO.Category;
using APIServer.DTO.CoverType;
using APIServer.DTO.Edition;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers.Manage
{
    [Route("api/[controller]")]
    public class EditionsController : ControllerBase
    {
        private readonly IEditionService _service;

        public EditionsController(IEditionService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<EditionResponse> GetAll()
        {
            return _service.GetAllAsQueryable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditionResponse>> GetById(int id)
        {
            var edition = await _service.GetByIdAsync(id);
            return edition == null ? NotFound() : Ok(edition);
        }

        [HttpPost]
        public async Task<ActionResult<EditionResponse>> Create([FromBody] EditionRequest dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.EditionId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EditionRequest dto)
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
