using APIServer.Data;
using APIServer.DTO.Author;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class AuthorService : IAuthorService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public AuthorService(LibraryDatabaseContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public IQueryable<AuthorRespone> GetAllAsQueryable()
        {
            return _context.Authors
                .Select(c => new AuthorRespone
                {
                    AuthorId = c.AuthorId,
                    AuthorName = c.AuthorName,
                    AuthorBio = c.Bio,
                    Nationality = c.Nationality,
                    Genre = c.Genre,
                    PhotoUrl = c.PhotoUrl,
                    BookCount = c.Books.Count()
                });
        }

        public async Task<Author?> UpdateAuthorAsync(int id, AuthorRequest request)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
                return null;

            if (request.Photo != null)
            {
                var uploadedUrl = await _cloudinaryService.UploadImageAsync(request.Photo, "authors");
                author.PhotoUrl = uploadedUrl;
            }

            author.AuthorName = request.AuthorName;
            author.Bio = request.AuthorBio;
            author.Nationality = request.Nationality;
            author.Genre = request.Genre;

            await _context.SaveChangesAsync();
            return author;
        }

        public async Task<AuthorRespone?> GetByIdAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return null;

            return new AuthorRespone
            {
                AuthorId = author.AuthorId,
                AuthorName = author.AuthorName,
                AuthorBio = author.Bio,
                Nationality = author.Nationality,
                Genre = author.Genre,
                PhotoUrl = author.PhotoUrl
            };
        }

        public async Task<Author> CreateAuthorAsync(AuthorRequest request)
        {
            string? photoUrl = null;

            if (request.Photo != null)
            {
                photoUrl = await _cloudinaryService.UploadImageAsync(request.Photo, "authors");
            }

            var author = new Author
            {
                AuthorName = request.AuthorName,
                Bio = request.AuthorBio,
                Nationality = request.Nationality,
                Genre = request.Genre,
                PhotoUrl = photoUrl
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return author;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return false;

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
