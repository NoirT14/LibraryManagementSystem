using APIServer.DTO.Book;

namespace APIServer.Service.Interfaces
{
    public interface IBookVariantService
    {
        Task<BookVariantReservationDTO?> GetVariantByIdAsync(int variantId);
    }
}
