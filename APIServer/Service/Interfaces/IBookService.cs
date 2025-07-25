using APIServer.DTO.Book;
using APIServer.Models;


namespace APIServer.Service.Interfaces
{
    public interface IBookService
    {
        IQueryable<Book> GetAll();
        Task<Book?> GetById(int id);
        Task<BookDetailRespone?> GetBookDetailById(int id);
        Task<Book> Create(BookInfoRequest request);
        Task<Book?> Update(int id, Microsoft.AspNetCore.OData.Deltas.Delta<Book> delta);
        Task<bool> Delete(int id);

        //the
        Task<int> CountTotalCopiesAsync();
        Task<Dictionary<string, int>> GetCopyStatusStatsAsync();

        Task<Dictionary<string, int>> GetBookCountByCategoryAsync();

        Task<List<BookHomepageDto>> GetBooksForHomepageAsync();

        Task<BookDetailDTO?> GetBookDetailAsync(int bookId);

    }
}
