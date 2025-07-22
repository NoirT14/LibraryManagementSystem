using APIServer.Data;
using APIServer.DTO.CoverType;
using APIServer.Models;
using APIServer.Service.Interfaces;
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

        public async Task<IEnumerable<CoverTypeResponse>> GetAllAsync()
        {
            return await _context.CoverTypes
                .Select(c => new CoverTypeResponse
                {
                    CoverTypeId = c.CoverTypeId,
                    CoverTypeName = c.CoverTypeName
                })
                .ToListAsync();
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
            var coverType = await _context.CoverTypes.FindAsync(id);
            if (coverType == null) return false;

            coverType.CoverTypeName = dto.CoverTypeName;

            _context.CoverTypes.Update(coverType);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var coverType = await _context.CoverTypes.FindAsync(id);
            if (coverType == null) return false;

            _context.CoverTypes.Remove(coverType);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
