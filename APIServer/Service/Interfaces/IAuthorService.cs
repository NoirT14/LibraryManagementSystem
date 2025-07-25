using APIServer.DTO.Author;
using APIServer.Models;

namespace APIServer.Service.Interfaces
{
    public interface IAuthorService
    {
        IQueryable<AuthorRespone> GetAllAsQueryable();
        Task<AuthorRespone?> GetByIdAsync(int id);
        Task<Author> CreateAuthorAsync(AuthorRequest request);
        Task<Author?> UpdateAuthorAsync(int id, AuthorRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
