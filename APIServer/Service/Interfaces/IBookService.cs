using APIServer.DTO.Book;

namespace APIServer.Service.Interfaces
{
    public interface IBookService
    {
        //The
        Task<List<HomepageBookDTO>> GetHomepageBooksAsync();
        Task<BookDetailDTO?> GetBookDetailByIdAsync(int id);

    }
}
