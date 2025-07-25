using APIServer.Data;
using APIServer.DTO.PaperQuality;
using APIServer.DTO.Publisher;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
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

        public IQueryable<PublisherResponse> GetAllAsQueryable()
        {
            return _context.Publishers
                .Select(p => new PublisherResponse
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Address = p.Address,
                    Phone = p.Phone,
                    Website = p.Website,
                    EstablishedYear = p.EstablishedYear,
                    BookCount = p.BookVariants
                        .SelectMany(bv => bv.BookCopies)
                        .Count()
                });
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
            if (string.IsNullOrEmpty(dto.PublisherName))
            {
                throw new InvalidOperationException("Publisher name is empty.");
            }

            if (StringHelper.ExistsInList(dto.PublisherName, _context.Publishers.Select(c => c.PublisherName).ToList())) throw new InvalidOperationException("Publisher already exists.");

            var entity = new Publisher
            {
                PublisherName = dto.PublisherName,
                Address = dto.Address,
                Phone = dto.Phone,
                Website = dto.Website,
                EstablishedYear = dto.EstablishedYear
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
            if (string.IsNullOrEmpty(dto.PublisherName))
            {
                throw new InvalidOperationException("Publisher name is empty.");
            }

            var entity = await _context.Publishers.FindAsync(id);
            if (entity == null) return false;

            if (StringHelper.ExistsInList(dto.PublisherName, _context.Publishers.Where(c => c.PublisherId != id).Select(c => c.PublisherName).ToList())) return false;

            entity.PublisherName = dto.PublisherName;
            entity.Address = dto.Address;
            entity.Phone = dto.Phone;
            entity.Website = dto.Website;
            entity.EstablishedYear = dto.EstablishedYear;

            _context.Publishers.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Publishers.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            _context.Publishers.Update(entity);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
