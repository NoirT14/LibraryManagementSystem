using APIServer.Data;
using APIServer.DTO.Category;
using APIServer.DTO.CoverType;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class CoverTypeService : ICoverTypeService
    {
        private readonly LibraryDatabaseContext _context;

        public CoverTypeService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public IQueryable<CoverTypeResponse> GetAllAsQueryable()
        {
            return _context.CoverTypes
                .Select(c => new CoverTypeResponse
                {
                    CoverTypeId = c.CoverTypeId,
                    CoverTypeName = c.CoverTypeName,
                    BookCount = c.BookVariants
                        .SelectMany(bv => bv.BookCopies)
                        .Count()
                });
        }

        public async Task<CoverTypeResponse?> GetByIdAsync(int id)
        {
            var coverType = await _context.CoverTypes.FindAsync(id);
            if (coverType == null) return null;

            return new CoverTypeResponse
            {
                CoverTypeId = coverType.CoverTypeId,
                CoverTypeName = coverType.CoverTypeName
            };
        }

        public async Task<CoverTypeResponse> CreateAsync(CoverTypeRequest dto)
        {
            if (string.IsNullOrEmpty(dto.CoverTypeName))
            {
                throw new InvalidOperationException("Cover type is empty.");
            }

            if (StringHelper.ExistsInList(dto.CoverTypeName, _context.CoverTypes.Select(c => c.CoverTypeName).ToList())) throw new InvalidOperationException("Cover type already exists.");

            var coverType = new CoverType
            {
                CoverTypeName = dto.CoverTypeName
            };

            _context.CoverTypes.Add(coverType);
            await _context.SaveChangesAsync();

            return new CoverTypeResponse
            {
                CoverTypeId = coverType.CoverTypeId,
                CoverTypeName = coverType.CoverTypeName
            };
        }

        public async Task<bool> UpdateAsync(int id, CoverTypeRequest dto)
        {
            if (string.IsNullOrEmpty(dto.CoverTypeName))
            {
                throw new InvalidOperationException("Cover type is empty.");
            }

            var coverType = await _context.CoverTypes.FindAsync(id);
            if (coverType == null) return false;

            if (StringHelper.ExistsInList(dto.CoverTypeName, _context.CoverTypes.Select(c => c.CoverTypeName).ToList())) return false;

            coverType.CoverTypeName = dto.CoverTypeName;

            _context.CoverTypes.Update(coverType);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var coverType = await _context.CoverTypes.FindAsync(id);
            if (coverType == null) return false;

            coverType.IsDeleted = true;
            _context.CoverTypes.Update(coverType);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
