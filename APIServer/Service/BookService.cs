using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System;

namespace APIServer.Service
{
    public class BookService : IBookService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        public BookService(LibraryDatabaseContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
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
            if (!string.IsNullOrWhiteSpace(request.AuthorIds))
            {
                var authorIds = request.AuthorIds.Split(',')
                                                 .Select(id => int.Parse(id.Trim()))
                                                 .ToList();

                book.Authors = await _context.Authors
                                             .Where(a => authorIds.Contains(a.AuthorId))
                                             .ToListAsync();
            }

            if (request.CoverImage != null)
            {
                    var uploadedUrl = await _cloudinaryService.UploadImageAsync(request.CoverImage, "books");
                    book.CoverImg = uploadedUrl;
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (request.Volumes != null && request.Volumes.Any())
            {
                var volumes = request.Volumes.Select(v => new BookVolume
                {
                    BookId = book.BookId,
                    VolumeNumber = v.VolumeNumber,
                    VolumeTitle = v.VolumeTitle,
                    Description = v.Description
                }).ToList();

                _context.BookVolumes.AddRange(volumes);
                await _context.SaveChangesAsync();
            }

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
            entity.isDelete = true;
            _context.Books.Update(entity);
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

        public IQueryable<BookInfoListSearchCopyRespone> GetBookInfoList(ODataQueryOptions<BookInfoListSearchCopyRespone> options)
        {
            var query = _context.BookVolumes
                .Include(v => v.Book)
                    .ThenInclude(b => b.Authors)
                .Where(v => !v.Book.isDelete)
                .Select(v => new BookInfoListSearchCopyRespone
                {
                    VolumeId = v.VolumeId,
                    VolumeNumber = v.VolumeNumber,
                    Title = v.Book.Title,
                    CoverImg = v.Book.CoverImg,
                    Author = string.Join(", ", v.Book.Authors.Select(a => a.AuthorName))
                });

            return (IQueryable<BookInfoListSearchCopyRespone>)options.ApplyTo(query);
        }

        public async Task<Book?> Update(int id, BookInfoRequest request)
        {
            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return null;

            // Cập nhật thông tin cơ bản
            book.Title = request.Title;
            book.Language = request.Language;
            book.Description = request.Description;
            book.CategoryId = request.CategoryId;
            book.BookStatus = request.BookStatus;

            // Cập nhật tác giả
            if (!string.IsNullOrWhiteSpace(request.AuthorIds))
            {
                var authorIds = request.AuthorIds.Split(',')
                                                 .Select(id => int.Parse(id.Trim()))
                                                 .ToList();

                var authors = await _context.Authors
                                            .Where(a => authorIds.Contains(a.AuthorId))
                                            .ToListAsync();

                book.Authors = authors;
            }

            // Cập nhật ảnh bìa nếu có
            if (request.CoverImage != null)
            {
                var uploadedUrl = await _cloudinaryService.UploadImageAsync(request.CoverImage, "books");
                book.CoverImg = uploadedUrl;
            }

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            if (request.Volumes != null)
            {
                foreach (var volReq in request.Volumes)
                {
                    if (volReq.VolumeId.HasValue)
                    {
                        // Cập nhật volume cũ
                        var existingVolume = book.BookVolumes.FirstOrDefault(v => v.VolumeId == volReq.VolumeId.Value);
                        if (existingVolume != null)
                        {
                            existingVolume.VolumeNumber = volReq.VolumeNumber;
                            existingVolume.VolumeTitle = volReq.VolumeTitle;
                            existingVolume.Description = volReq.Description;
                        }
                    }
                    else
                    {
                        // Thêm volume mới
                        var newVolume = new BookVolume
                        {
                            BookId = book.BookId,
                            VolumeNumber = volReq.VolumeNumber,
                            VolumeTitle = volReq.VolumeTitle,
                            Description = volReq.Description
                        };
                        _context.BookVolumes.Add(newVolume);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                .FirstAsync(b => b.BookId == book.BookId);
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
  
        // BookService.cs
        public async Task<List<BookHomepageDto>> GetBooksForHomepageAsync()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(v => v.BookCopies)
                .Where(b => b.BookStatus == "Active")
                .Select(b => new BookHomepageDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Category = b.Category.CategoryName,
                    Language = b.Language,
                    Authors = b.Authors.Select(ba => ba.AuthorName).ToList(),
                    Image = b.CoverImg,
                    Available = b.BookVolumes
                                  .SelectMany(v => v.BookVariants)
                                  .SelectMany(v => v.BookCopies)
                                  .Any(c => c.CopyStatus == "Available")
                })
                .ToListAsync();

            return books;
        }

        public async Task<BookDetailDTO?> GetBookDetailAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(v => v.BookCopies)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(v => v.Publisher)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(v => v.CoverType)
                .Include(b => b.BookVolumes)
                    .ThenInclude(v => v.BookVariants)
                        .ThenInclude(v => v.PaperQuality)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null) return null;

            var dto = new BookDetailDTO
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                Language = book.Language,
                Status = book.BookStatus,
                CategoryName = book.Category?.CategoryName,
                Image = book.CoverImg, 
                Authors = book.Authors.Select(ba => ba.AuthorName).ToList(),
                Variants = book.BookVolumes
                    .SelectMany(v => v.BookVariants.Select(variant => new BookDetailDTO.VariantInfo
                    {
                        VariantId = variant.VariantId,
                        PublicationYear = (int)variant.PublicationYear,
                        ISBN = variant.Isbn,
                        PublisherName = variant.Publisher?.PublisherName,
                        CoverTypeName = variant.CoverType?.CoverTypeName,
                        PaperQualityName = variant.PaperQuality?.PaperQualityName,
                        VolumeTitle = v.VolumeTitle,
                        TotalCopies = variant.BookCopies.Count,
                        AvailableCopies = variant.BookCopies.Count(c => c.CopyStatus == "Available")
                    }))
                    .ToList()
            };

            return dto;
        }

        public async Task<BookAllFieldRespone?> GetBookAllFieldAsync(int bookId)
        {
            var book = await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Category)
            .Include(b => b.BookVolumes)
                .ThenInclude(v => v.BookVariants)
                    .ThenInclude(v => v.BookCopies)
            .Include(b => b.BookVolumes)
                .ThenInclude(v => v.BookVariants)
                    .ThenInclude(v => v.Edition)
            .Include(b => b.BookVolumes)
                .ThenInclude(v => v.BookVariants)
                    .ThenInclude(v => v.Publisher)
            .Include(b => b.BookVolumes)
                .ThenInclude(v => v.BookVariants)
                    .ThenInclude(v => v.CoverType)
            .Include(b => b.BookVolumes)
                .ThenInclude(v => v.BookVariants)
                    .ThenInclude(v => v.PaperQuality)
            .FirstOrDefaultAsync(b => b.BookId == bookId && !b.isDelete);

            if (book == null) return null;

            return new BookAllFieldRespone
            {
                BookId = book.BookId,
                Title = book.Title,
                Language = book.Language,
                BookStatus = book.BookStatus,
                Description = book.Description,
                CoverImg = book.CoverImg,
                CategoryName = book.Category.CategoryName,
                Authors = book.Authors.Select(a => a.AuthorName).ToList(),
                Volumes = book.BookVolumes.Select(v => new BookAllFieldRespone.VolumeDto
                {
                    VolumeId = v.VolumeId,
                    VolumeNumber = v.VolumeNumber,
                    VolumeTitle = v.VolumeTitle,
                    Description = v.Description,
                    Variants = v.BookVariants.Select(variant => new BookAllFieldRespone.VariantDto
                    {
                        VariantId = variant.VariantId,
                        PublicationYear = variant.PublicationYear,
                        Isbn = variant.Isbn,
                        EditionName = variant.Edition?.EditionName,
                        PublisherName = variant.Publisher?.PublisherName,
                        CoverTypeName = variant.CoverType?.CoverTypeName,
                        PaperQualityName = variant.PaperQuality?.PaperQualityName,
                        Copies = variant.BookCopies.Select(c => new BookAllFieldRespone.CopyDto
                        {
                            CopyId = c.CopyId,
                            Barcode = c.Barcode,
                            CopyStatus = c.CopyStatus,
                            Location = c.Location
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }
    }
}
