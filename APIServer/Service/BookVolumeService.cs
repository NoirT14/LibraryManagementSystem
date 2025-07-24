using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Services
{
    public class BookVolumeService : IBookVolumeService
    {
        private readonly LibraryDatabaseContext _context;

        public BookVolumeService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<BookVolumeDTO>> GetAllAsync()
        {
            return await _context.BookVolumes
                .Include(v => v.Book)
                .Select(v => new BookVolumeDTO
                {
                    VolumeId = v.VolumeId,
                    VolumeNumber = v.VolumeNumber,
                    VolumeTitle = v.VolumeTitle,
                    Description = v.Description,
                }).ToListAsync();
        }

        public async Task<BookVolumeDTO?> GetByIdAsync(int id)
        {
            return await _context.BookVolumes
                .Include(v => v.Book)
                .Where(v => v.VolumeId == id)
                .Select(v => new BookVolumeDTO
                {
                    VolumeId = v.VolumeId,
                    VolumeNumber = v.VolumeNumber,
                    VolumeTitle = v.VolumeTitle,
                    Description = v.Description,
                }).FirstOrDefaultAsync();
        }
    }
}
