using APIServer.Models;

namespace APIServer.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<User>> GetAllWithRolesAsync();
        Task<User?> GetByIdWithRoleAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<(List<User> Items, int TotalCount)> GetUsersWithRolesPagedAsync(int page, int pageSize);
    }
}
