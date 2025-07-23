using APIServer.DTO.Book;

namespace APIServer.Service.Interfaces
{
    public interface IBookService
    {
       
        Task<BookDetailDTO?> GetBookDetailByIdAsync(int id);

        //the
        Task<int> CountTotalCopiesAsync();
        Task<Dictionary<string, int>> GetCopyStatusStatsAsync();

        Task<Dictionary<string, int>> GetBookCountByCategoryAsync();

    }
}
