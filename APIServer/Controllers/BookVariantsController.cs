using APIServer.DTO.Book;
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

       

        [HttpGet("{variantId}")]
        public async Task<ActionResult<BookVariantDto>> GetBookVariantWithBook(int variantId)
        {
            var dto = await _variantService.GetBookVariantWithBookAsync(variantId);
            if (dto == null)
                return NotFound($"No BookVariant found with VariantId = {variantId}");

            return Ok(dto);
        }

    }

}
