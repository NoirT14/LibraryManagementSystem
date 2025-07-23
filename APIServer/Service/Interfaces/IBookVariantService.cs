using APIServer.DTO.Book;

namespace APIServer.Service.Interfaces
{
    public interface IBookVariantService
    {

        Task<BookVariantDto?> GetBookVariantWithBookAsync(int variantId);
    }
}
