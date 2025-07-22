using APIServer.DTO.Author;

namespace APIServer.Service.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorRespone?>> GetAllAsync();
        Task<AuthorRespone?> GetByIdAsync(int id);
        Task<AuthorRespone> CreateAsync(AuthorRequest dto);
        Task<bool> UpdateAsync(int id, AuthorRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
