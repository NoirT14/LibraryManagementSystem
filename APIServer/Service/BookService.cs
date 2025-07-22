using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class BookService : IBookService
    {
        private readonly LibraryDatabaseContext _context;
        public BookService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Book> Create(BookInfoRequest request)
        {
            var book = new Book
            {
                Title = request.Title,
                CategoryId = request.CategoryId,
                Language = request.Language,
                BookStatus = request.BookStatus,
                Description = request.Description,
            };

            // Gán tác giả nếu có
            if (request.AuthorIds != null && request.AuthorIds.Any())
            {
                book.Authors = await _context.Authors
                    .Where(a => request.AuthorIds.Contains(a.AuthorId))
                    .ToListAsync();
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Load navigation properties để trả về đầy đủ
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                .FirstAsync(b => b.BookId == book.BookId);
        }


        public async Task<bool> Delete(int id)
        {
            var entity = await _context.Books.FindAsync(id);
            if (entity == null) return false;
            _context.Books.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<Book> GetAll()
        {
            return _context.Books
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(variant => variant.BookCopies);
        }

        public static BookInfoRespone ToDto(Book book)
        {
            return new BookInfoRespone
            {
                BookId = book.BookId,
                Title = book.Title,
                Language = book.Language,
                BookStatus = book.BookStatus,
                Description = book.Description,
                Author = string.Join(", ", book.Authors.Select(a => a.AuthorName)),
                Volumn = book.BookVolumes.Count.ToString(),
                Availability = GetAvailability(book.BookVolumes),
                CoverImg = book.CoverImg,
            };
        }


        private static string GetAvailability(ICollection<BookVolume> volumes)
        {
            int totalCopies = 0;
            int availableCopies = 0;

            foreach (var volume in volumes)
            {
                foreach (var variant in volume.BookVariants)
                {
                    var copies = variant.BookCopies;
                    totalCopies += copies.Count;
                    availableCopies += copies.Count(c => c.CopyStatus == "Available");
                }
            }

            return $"{availableCopies}/{totalCopies}";
        }

        public async Task<Book?> GetById(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task<BookDetailRespone?> GetBookDetailById(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants!)
                        .ThenInclude(v => v.Publisher)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants!)
                        .ThenInclude(v => v.CoverType)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants!)
                        .ThenInclude(v => v.PaperQuality)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants!)
                        .ThenInclude(v => v.BookCopies) // để lấy location
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return null;

            var variantDtos = book.BookVolumes
    .SelectMany(v => v.BookVariants ?? new List<BookVariant>())
    .Select(variant =>
    {
        var availableCopies = variant.BookCopies?
            .Where(copy => copy.CopyStatus == "Available")
            .ToList();

        string location;
        if (availableCopies != null && availableCopies.Any())
        {
            location = string.Join("\n", availableCopies
       .Select(copy => $"{copy.Location} - Available"));
        }
        else
        {
            location = "All book are borrowed";
        }

        return new BookVariantDetailDTO
        {
            VariantId = variant.VariantId,
            Publisher = variant.Publisher?.PublisherName ?? "N/A",
            ISBN = variant.Isbn,
            PublicationYear = variant.PublicationYear,
            CoverType = variant.CoverType?.CoverTypeName,
            PaperQuality = variant.PaperQuality?.PaperQualityName,
            Price = variant.Price,
            Location = location
        };
    }).ToList();


            return new BookDetailRespone
            {
                BookId = book.BookId,
                Title = book.Title,
                Language = book.Language,
                BookStatus = book.BookStatus,
                Description = book.Description,
                CoverImg = book.CoverImg,
                CategoryName = book.Category?.CategoryName ?? "N/A",
                AuthorNames = book.Authors.Select(a => a.AuthorName).ToList(),
                Volumes = book.BookVolumes.Select(v => new BookVolumeDTO
                {
                    VolumeId = v.VolumeId,
                    VolumeNumber = v.VolumeNumber,
                    VolumeTitle = v.VolumeTitle,
                    Description = v.Description
                }).ToList(),
                Variants = variantDtos
            };
        }


        public async Task<Book?> Update(int id, Delta<Book> delta)
        {
            var entity = await _context.Books.FindAsync(id);
            if (entity == null) return null;

            delta.Patch(entity);
            await _context.SaveChangesAsync();

            // Load navigation properties sau khi update
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }
    }
}
