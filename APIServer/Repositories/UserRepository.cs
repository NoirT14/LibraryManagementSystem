using APIServer.Data;
using APIServer.Models;
using APIServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace APIServer.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(LibraryDatabaseContext context) : base(context) { }

        public async Task<List<User>> GetAllWithRolesAsync()
        {
            return await _dbSet.Include(u => u.Role).ToListAsync();
        }

        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}
