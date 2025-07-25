using APIServer.Data;
using APIServer.DTO.CoverType;
using APIServer.DTO.Edition;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class EditionService : IEditionService
    {
        private readonly LibraryDatabaseContext _context;

        public EditionService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public IQueryable<EditionResponse> GetAllAsQueryable()
        {
            return _context.Editions
                .Select(c => new EditionResponse
                {
                    EditionId = c.EditionId,
                    EditionName = c.EditionName,
                    BookCount = c.BookVariants
                        .SelectMany(bv => bv.BookCopies)
                        .Count()
                });
        }

        public async Task<EditionResponse?> GetByIdAsync(int id)
        {
            var edition = await _context.Editions.FindAsync(id);
            if (edition == null) return null;

            return new EditionResponse
            {
                EditionId = edition.EditionId,
                EditionName = edition.EditionName
            };
        }

        public async Task<EditionResponse> CreateAsync(EditionRequest dto)
        {
            if (string.IsNullOrEmpty(dto.EditionName))
            {
                throw new InvalidOperationException("Edition is empty.");
            }

            if (StringHelper.ExistsInList(dto.EditionName, _context.Editions.Select(c => c.EditionName).ToList())) throw new InvalidOperationException("Edition already exists.");

            var edition = new Edition
            {
                EditionName = dto.EditionName
            };

            _context.Editions.Add(edition);
            await _context.SaveChangesAsync();

            return new EditionResponse
            {
                EditionId = edition.EditionId,
                EditionName = edition.EditionName
            };
        }

        public async Task<bool> UpdateAsync(int id, EditionRequest dto)
        {
            if (string.IsNullOrEmpty(dto.EditionName))
            {
                throw new InvalidOperationException("Edition is empty.");
            }

            var edition = await _context.Editions.FindAsync(id);

            if (edition == null) return false;

            if(StringHelper.ExistsInList(dto.EditionName, _context.Editions.Select(c => c.EditionName).ToList())) return false;

            edition.EditionName = dto.EditionName;

            _context.Editions.Update(edition);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var edition = await _context.Editions.FindAsync(id);
            if (edition == null) return false;

            edition.IsDeleted = true;
            _context.Editions.Update(edition);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}