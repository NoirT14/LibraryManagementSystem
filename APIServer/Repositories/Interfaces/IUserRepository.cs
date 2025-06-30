using APIServer.Models;

namespace APIServer.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<User>> GetAllWithRolesAsync();
        Task<User?> GetByIdWithRoleAsync(int userId);
    }
}
