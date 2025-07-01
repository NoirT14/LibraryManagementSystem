using APIServer.DTO.Book;
using APIServer.Models;
using APIServer.Repositories.Interfaces;
using APIServer.Service.Interfaces;

namespace APIServer.Service
{
    public class BookVariantService : IBookVariantService
    {
        private readonly IRepository<BookVariant> _variantRepo;

        public BookVariantService(IRepository<BookVariant> variantRepo)
        {
            _variantRepo = variantRepo;
        }

        public async Task<BookVariantReservationDTO?> GetVariantByIdAsync(int variantId)
        {
            var variant = await _variantRepo.FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
                return null;

            // Nếu không eager load Volume thì dùng lazy hoặc query Volume
            var dto = new BookVariantReservationDTO
            {
                VariantId = variant.VariantId,
                ISBN = variant.Isbn,
                PublicationYear = variant.PublicationYear,
                Title = variant.Volume?.VolumeTitle ?? "[Không rõ tiêu đề]"
            };

            return dto;
        }
    }

}
