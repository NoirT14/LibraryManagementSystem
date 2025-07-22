using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.Models;
using APIServer.Repositories.Interfaces;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class BookVariantService : IBookVariantService
    {
        private readonly LibraryDatabaseContext _context;
        public BookVariantService(LibraryDatabaseContext context)
        {
            _context = context;
        }

       
        public async Task<BookVariantDto?> GetBookVariantWithBookAsync(int variantId)
        {
            var variant = await _context.BookVariants
                .Include(v => v.Volume)
                    .ThenInclude(vol => vol.Book)
                        .ThenInclude(book => book.Authors)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null) return null;

            return new BookVariantDto
            {
                VariantId = variant.VariantId,
                VolumeTitle = variant.Volume.VolumeTitle,
                VolumeNumber = variant.Volume.VolumeNumber,
                BookTitle = variant.Volume.Book.Title,
                Authors = variant.Volume.Book.Authors.Select(a => a.AuthorName).ToList()
            };
        }

    }

}
