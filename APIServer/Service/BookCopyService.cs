using APIServer.Data;
using APIServer.DTO;
using APIServer.DTO.Book;
using APIServer.DTO.Loans;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class BookCopyService : IBookCopyService
    {
        private readonly LibraryDatabaseContext _context;

        public BookCopyService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public PagedResult<BookCopyInfoListDto> GetFilteredBookCopies(ODataQueryOptions<BookCopyInfoListDto> options)
        {
            var rawData = _context.BookCopies
             .Include(b => b.Variant)
                 .ThenInclude(v => v.CoverType)
             .Include(b => b.Variant.Edition)
             .Include(b => b.Variant.Publisher)
             .Include(b => b.Variant.PaperQuality)
             .Include(b => b.Variant.Volume)
                 .ThenInclude(vol => vol.Book)
                     .ThenInclude(book => book.Authors)
             .Include(b => b.Variant.Volume.Book.Category)
             .AsEnumerable()
             .Select(b => new BookCopyInfoListDto
             {
                 CopyId = b.CopyId,
                 Barcode = b.Barcode,
                 CopyStatus = b.CopyStatus,
                 Location = b.Location,
                 Volumn = b.Variant.Volume.VolumeNumber,

                 VariantId = b.Variant.VariantId,
                 PublicationYear = b.Variant.PublicationYear,
                 Isbn = b.Variant.Isbn,

                 CoverTypeName = b.Variant.CoverType?.CoverTypeName,
                 EditionName = b.Variant.Edition?.EditionName,
                 PublisherName = b.Variant.Publisher?.PublisherName,
                 PaperQualityName = b.Variant.PaperQuality?.PaperQualityName,

                 BookTitle = b.Variant.Volume.Book.Title,
                 CategoryName = b.Variant.Volume.Book.Category.CategoryName,
                 AuthorNames = b.Variant.Volume.Book.Authors.Select(a => a.AuthorName).ToList()
             });

            var filteredData = (IQueryable<BookCopyInfoListDto>)options.ApplyTo(
                rawData.AsQueryable(),
                ignoreQueryOptions: AllowedQueryOptions.Skip | AllowedQueryOptions.Top
            );

            int totalCount = filteredData.Count();

            int skip = options.Skip?.Value ?? 0;
            int top = options.Top?.Value ?? 10;

            var pagedItems = filteredData.Skip(skip).Take(top).ToList();

            return new PagedResult<BookCopyInfoListDto>
            {
                TotalCount = totalCount,
                Items = pagedItems
            };
        }

        public async Task<object?> CreateBookCopyFullAsync(BookCopyRequest request)
        {
            var volume = await _context.BookVolumes
                .Include(v => v.Book)
                .FirstOrDefaultAsync(v => v.VolumeId == request.VolumeId);

            if (volume == null)
                return null;

            var variant = new BookVariant
            {
                VolumeId = request.VolumeId,
                PublisherId = request.PublisherId,
                EditionId = request.EditionId,
                PublicationYear = request.PublicationYear,
                CoverTypeId = request.CoverTypeId,
                PaperQualityId = request.PaperQualityId,
                Isbn = StringHelper.GenerateIsbn(),
                Notes = request.Notes
            };

            _context.BookVariants.Add(variant);
            await _context.SaveChangesAsync();

            var copy = new BookCopy
            {
                VariantId = variant.VariantId,
                Barcode = StringHelper.GenerateBarcode(),
                CopyStatus = request.CopyStatus ?? "Available",
                Location = request.Location
            };

            _context.BookCopies.Add(copy);
            await _context.SaveChangesAsync();

            return new
            {
                copy.CopyId,
                variant.VariantId,
                volume.VolumeId,
                volume.BookId,
                volume.Book.Title,
                copy.Barcode,
                variant.Isbn,
                copy.CopyStatus,
                copy.Location
            };
        }

        public async Task<bool> UpdateCopyAndVariantAsync(int copyId, UpdateCopyAndVariantRequest request)
        {
            var copy = await _context.BookCopies
                .Include(c => c.Variant)
                .FirstOrDefaultAsync(c => c.CopyId == copyId);

            if (copy == null)
                return false;

            // Update variant
            if (request.CoverTypeId.HasValue)
                copy.Variant.CoverTypeId = request.CoverTypeId;

            if (request.PaperQualityId.HasValue)
                copy.Variant.PaperQualityId = request.PaperQualityId;

            if (request.EditionId.HasValue)
                copy.Variant.EditionId = request.EditionId;

            // Update copy
            if (!string.IsNullOrWhiteSpace(request.Location))
                copy.Location = request.Location;

            if (!string.IsNullOrWhiteSpace(request.CopyStatus))
                copy.CopyStatus = request.CopyStatus;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
