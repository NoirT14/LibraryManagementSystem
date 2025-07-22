using APIServer.Data;
using APIServer.DTO.PaperQuality;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace APIServer.Service
{
    public class PaperQualityService : IPaperQualityService
    {
        private readonly LibraryDatabaseContext _context;

        public PaperQualityService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaperQualityResponse>> GetAllAsync()
        {
            return await _context.PaperQualities
                .Select(p => new PaperQualityResponse
                {
                    PaperQualityId = p.PaperQualityId,
                    PaperQualityName = p.PaperQualityName
                })
                .ToListAsync();
        }

        public async Task<PaperQualityResponse?> GetByIdAsync(int id)
        {
            var paperQuality = await _context.PaperQualities.FindAsync(id);
            if (paperQuality == null) return null;

            return new PaperQualityResponse
            {
                PaperQualityId = paperQuality.PaperQualityId,
                PaperQualityName = paperQuality.PaperQualityName
            };
        }

        public async Task<PaperQualityResponse> CreateAsync(PaperQualityRequest dto)
        {
            var entity = new PaperQuality
            {
                PaperQualityName = dto.PaperQualityName
            };

            _context.PaperQualities.Add(entity);
            await _context.SaveChangesAsync();

            return new PaperQualityResponse
            {
                PaperQualityId = entity.PaperQualityId,
                PaperQualityName = entity.PaperQualityName
            };
        }

        public async Task<bool> UpdateAsync(int id, PaperQualityRequest dto)
        {
            var entity = await _context.PaperQualities.FindAsync(id);
            if (entity == null) return false;

            entity.PaperQualityName = dto.PaperQualityName;

            _context.PaperQualities.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PaperQualities.FindAsync(id);
            if (entity == null) return false;

            _context.PaperQualities.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
