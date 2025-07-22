using APIServer.DTO.PaperQuality;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IEnumerable<PaperQualityResponse>>> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<PaperQualityResponse>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<PaperQualityResponse>> Create(PaperQualityRequest dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.PaperQualityId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PaperQualityRequest dto)
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
