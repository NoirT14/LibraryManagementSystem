using APIServer.DTO.Book;
using APIServer.Models;
using Microsoft.AspNetCore.OData.Query;


namespace APIServer.Service.Interfaces
{
    public interface IBookService
    {
        IQueryable<Book> GetAll();
        Task<Book?> GetById(int id);
        Task<BookDetailRespone?> GetBookDetailById(int id);
        Task<Book> Create(BookInfoRequest request);
        Task<Book?> Update(int id, BookInfoRequest request);
        Task<bool> Delete(int id);
        Task<BookAllFieldRespone?> GetBookAllFieldAsync(int bookId);
        IQueryable<BookInfoListSearchCopyRespone> GetBookInfoList(ODataQueryOptions<BookInfoListSearchCopyRespone> options);

        //the
        Task<int> CountTotalCopiesAsync();
        Task<Dictionary<string, int>> GetCopyStatusStatsAsync();

        Task<Dictionary<string, int>> GetBookCountByCategoryAsync();

        Task<List<BookHomepageDto>> GetBooksForHomepageAsync();

        Task<BookDetailDTO?> GetBookDetailAsync(int bookId);


    }
}
