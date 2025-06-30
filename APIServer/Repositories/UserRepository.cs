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
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }
    }
}
