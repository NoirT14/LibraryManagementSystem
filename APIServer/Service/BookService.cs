using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace APIServer.Services
{
    public class BookService : IBookService
    {
        private readonly LibraryDatabaseContext _context;

        public BookService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        

        public async Task<BookDetailDTO?> GetBookDetailByIdAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(variant => variant.BookCopies)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(variant => variant.CoverType)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(variant => variant.Publisher)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(variant => variant.PaperQuality)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return null;

            return new BookDetailDTO
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                Language = book.Language,
                Status = book.BookStatus,
                CategoryName = book.Category?.CategoryName,
                Authors = book.Authors.Select(a => a.AuthorName).ToList(),
                Variants = book.BookVolumes
                    .SelectMany(v => v.BookVariants)
                    .Select(variant => new BookDetailDTO.VariantInfo
                    {
                        VariantId = variant.VariantId,
                        PublicationYear = variant.PublicationYear ?? 0,
                        ISBN = variant.Isbn,
                        PublisherName = variant.Publisher?.PublisherName,
                        CoverTypeName = variant.CoverType?.CoverTypeName,
                        PaperQualityName = variant.PaperQuality?.PaperQualityName,
                        TotalCopies = variant.BookCopies.Count(),
                        AvailableCopies = variant.BookCopies.Count(c => c.CopyStatus == "Available")
                    })
                    .ToList()
            };
        }



        //The
        public async Task<int> CountTotalCopiesAsync()
        {
            return await _context.BookCopies.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetCopyStatusStatsAsync()
        {
            return await _context.BookCopies
                .GroupBy(c => c.CopyStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }
        public async Task<Dictionary<string, int>> GetBookCountByCategoryAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .GroupBy(b => b.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Category, g => g.Count);
        }
    }
}
