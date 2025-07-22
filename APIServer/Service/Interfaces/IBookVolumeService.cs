using APIServer.DTO.Book;

namespace APIServer.Service.Interfaces
{
    public interface IBookVolumeService
    {
        Task<List<BookVolumeDTO>> GetAllAsync();
        Task<BookVolumeDTO?> GetByIdAsync(int id);
    }
}
