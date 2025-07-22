using APIServer.DTO.Book;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers.OData
{
    [Route("odata/BookVolumes")]
    [ApiExplorerSettings(IgnoreApi = true)]

    public class BookVolumesController : ODataController

    {
        private readonly IBookVolumeService _volumeService;

        public BookVolumesController(IBookVolumeService volumeService)
        {
            _volumeService = volumeService;
        }

        [EnableQuery]
        [HttpGet]
        public async Task<ActionResult<IQueryable<BookVolumeDTO>>> Get()
        {
            var volumes = await _volumeService.GetAllAsync();
            return Ok(volumes.AsQueryable());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookVolumeDTO>> Get(int id)
        {
            var volume = await _volumeService.GetByIdAsync(id);
            if (volume == null)
                return NotFound(new { error = "Không tìm thấy tập sách." });

            return Ok(volume);
        }
    }
}
