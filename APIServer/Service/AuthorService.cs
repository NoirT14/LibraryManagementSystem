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

        public AuthorService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuthorRespone>> GetAllAsync()
        {
            return await _context.Authors
                .Select(c => new AuthorRespone
                {
                    AuthorId = c.AuthorId,
                    AuthorName = c.AuthorName,
                    AuthorBio = c.Bio,
                })
                .ToListAsync();
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
            };
        }

        public async Task<AuthorRespone> CreateAsync(AuthorRequest dto)
        {
            var author = new Author
            {
                AuthorName = dto.AuthorName,
                Bio = dto.AuthorBio,
                
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return new AuthorRespone
            {
                AuthorId = author.AuthorId,
                AuthorName = author.AuthorName,
                AuthorBio = author.Bio,
            };
        }

        public async Task<bool> UpdateAsync(int id, AuthorRequest dto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return false;

            author.AuthorName = dto.AuthorName;
            author.Bio = dto.AuthorBio;

            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
            return true;
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
