using APIServer.Data;
using APIServer.DTO.Edition;
using APIServer.DTO.PaperQuality;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
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

        public IQueryable<PaperQualityResponse> GetAllAsQueryable()
        {
            return _context.PaperQualities
                .Select(p => new PaperQualityResponse
                {
                    PaperQualityId = p.PaperQualityId,
                    PaperQualityName = p.PaperQualityName,
                    BookCount = p.BookVariants
                        .SelectMany(bv => bv.BookCopies)
                        .Count()
                });
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
            if (string.IsNullOrEmpty(dto.PaperQualityName))
            {
                throw new InvalidOperationException("Paper name is empty.");
            }

            if (StringHelper.ExistsInList(dto.PaperQualityName, _context.PaperQualities.Select(c => c.PaperQualityName).ToList())) throw new InvalidOperationException("Paper Quality already exists.");

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
            if (string.IsNullOrEmpty(dto.PaperQualityName))
            {
                throw new InvalidOperationException("Paper name is empty.");
            }

            var entity = await _context.PaperQualities.FindAsync(id);
            if (entity == null) return false;
            if (StringHelper.ExistsInList(dto.PaperQualityName, _context.PaperQualities.Select(c => c.PaperQualityName).ToList())) return false;

            entity.PaperQualityName = dto.PaperQualityName;

            _context.PaperQualities.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PaperQualities.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            _context.PaperQualities.Update(entity);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
