using APIServer.DTO.Edition;
using APIServer.DTO.PaperQuality;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers.Manage
{
    [Route("api/[controller]")]
    public class PaperQualitiesController : ControllerBase
    {
        private readonly IPaperQualityService _service;

        public PaperQualitiesController(IPaperQualityService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<PaperQualityResponse> GetAll()
        {
            return _service.GetAllAsQueryable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaperQualityResponse>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<PaperQualityResponse>> Create([FromBody] PaperQualityRequest dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PaperQualityId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaperQualityRequest dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return updated ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
