using APIServer.Data;
using APIServer.DTO.Publisher;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace APIServer.Service
{
    public class PublisherService : IPublisherService
    {
        private readonly LibraryDatabaseContext _context;

        public PublisherService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PublisherResponse>> GetAllAsync()
        {
            return await _context.Publishers
                .Select(p => new PublisherResponse
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName
                })
                .ToListAsync();
        }

        public async Task<PublisherResponse?> GetByIdAsync(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null) return null;

            return new PublisherResponse
            {
                PublisherId = publisher.PublisherId,
                PublisherName = publisher.PublisherName
            };
        }

        public async Task<PublisherResponse> CreateAsync(PublisherRequest dto)
        {
            var entity = new Publisher
            {
                PublisherName = dto.PublisherName
            };

            _context.Publishers.Add(entity);
            await _context.SaveChangesAsync();

            return new PublisherResponse
            {
                PublisherId = entity.PublisherId,
                PublisherName = entity.PublisherName
            };
        }

        public async Task<bool> UpdateAsync(int id, PublisherRequest dto)
        {
            var entity = await _context.Publishers.FindAsync(id);
            if (entity == null) return false;

            entity.PublisherName = dto.PublisherName;

            _context.Publishers.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Publishers.FindAsync(id);
            if (entity == null) return false;

            _context.Publishers.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
