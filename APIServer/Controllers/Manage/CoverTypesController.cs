using APIServer.DTO.Category;
using APIServer.DTO.CoverType;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers.Manage
{
    [Route("api/[controller]")]
    public class CoverTypesController : ControllerBase
    {
        private readonly ICoverTypeService _service;

        public CoverTypesController(ICoverTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<CoverTypeResponse> GetAll()
        {
            return _service.GetAllAsQueryable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CoverTypeResponse>> GetById(int id)
        {
            var coverType = await _service.GetByIdAsync(id);
            return coverType == null ? NotFound() : Ok(coverType);
        }

        [HttpPost]
        public async Task<ActionResult<CoverTypeResponse>> Create([FromBody] CoverTypeRequest dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.CoverTypeId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CoverTypeRequest dto)
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
