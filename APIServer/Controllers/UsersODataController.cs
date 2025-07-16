using APIServer.Data;
using APIServer.DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("odata/[controller]")]
    public class UsersODataController : ODataController
    {
        private readonly LibraryDatabaseContext _context;

        public UsersODataController(LibraryDatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET: /odata/UsersOData
        [EnableQuery]
        [HttpGet]
        public IQueryable<AdminUserResponseDTO> GetAllUsers()
        {
            return _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Select(u => new AdminUserResponseDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    IsActive = u.IsActive,
                    CreateDate = u.CreateDate,
                    RoleName = u.Role.RoleName
                });
        }

        // ✅ GET: /odata/UsersOData(1)
        [EnableQuery]
        [HttpGet("{key}")]
        public SingleResult<AdminUserResponseDTO> GetUserById([FromRoute] int key)
        {
            var result = _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Where(u => u.UserId == key)
                .Select(u => new AdminUserResponseDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    IsActive = u.IsActive,
                    CreateDate = u.CreateDate,
                    RoleName = u.Role.RoleName
                });

            return SingleResult.Create(result);
        }
    }
}
