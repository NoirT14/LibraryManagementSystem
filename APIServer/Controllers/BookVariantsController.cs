using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookVariantsController : ControllerBase
    {
        private readonly IBookVariantService _variantService;

        public BookVariantsController(IBookVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _variantService.GetVariantByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }

}
