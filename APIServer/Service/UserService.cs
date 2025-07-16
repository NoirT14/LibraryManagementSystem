using APIServer.DTO.User;
using APIServer.Models;
using APIServer.Repositories.Interfaces;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace APIServer.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //User
        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDTO changePasswordRequest)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordRequest.CurrentPassword);
            if(result != PasswordVerificationResult.Success)
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }
            if(changePasswordRequest.NewPassword != changePasswordRequest.ConfirmNewPassword) throw new ArgumentException("Passwords do not match");

            user.PasswordHash = passwordHasher.HashPassword(user, changePasswordRequest.NewPassword);
            
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }
        public async Task UpdateProfileAsync(int userId, UserProfileUpdateRequestDTO userProfileUpdateRequest)
        {
            var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

            user.FullName = userProfileUpdateRequest.FullName;
            user.Phone = userProfileUpdateRequest.Phone;
            user.Address = userProfileUpdateRequest.Address;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }
        public async Task<UserProfileResponseDTO> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

            return new UserProfileResponseDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                CreateDate = user.CreateDate,
            };
        }

        //Admin
        public async Task<AdminUserResponseDTO> CreateUserAsync(CreateUserRequestDTO createUserRequest)
        {
            var user = new User
            {
                Username = createUserRequest.Username,
                FullName = createUserRequest.FullName,
                Email = createUserRequest.Email,
                Phone = createUserRequest.Phone,
                Address = createUserRequest.Address,
                RoleId = createUserRequest.RoleId,
                IsActive = true,
                CreateDate = DateTime.UtcNow
            };
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, createUserRequest.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            
            var created = await _userRepository.GetByIdWithRoleAsync(user.UserId);

            return new AdminUserResponseDTO
            {
                UserId = created.UserId,
                Username = created.Username,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                Address = created.Address,
                RoleName = created.Role?.RoleName,
                IsActive = created.IsActive,
                CreateDate = created.CreateDate
            };
        }

        public async Task<List<AdminUserResponseDTO>> GetAllUserAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            return users.Select(x => new AdminUserResponseDTO
            {
                UserId = x.UserId,
                Username = x.Username,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                RoleName = x.Role?.RoleName,
                IsActive = x.IsActive,
                CreateDate = x.CreateDate,
            }).ToList();
        }

        public async Task ResetPasswordAsync(AdminResetPasswordRequestDTO resetPasswordRequest)
        {
            var user = await _userRepository.GetByIdAsync(resetPasswordRequest.TargetUserId)
            ?? throw new KeyNotFoundException("User not found");

            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, resetPasswordRequest.NewPassword);

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task UpdateUserByAdminAsync(AdminUpdateUserRequestDTO updateUserByAdminRequest)
        {
            var user = await _userRepository.GetByIdAsync(updateUserByAdminRequest.UserId)
            ?? throw new KeyNotFoundException("User not found");

            user.FullName = updateUserByAdminRequest.FullName;
            user.Email = updateUserByAdminRequest.Email;
            user.Phone = updateUserByAdminRequest.Phone;
            user.Address = updateUserByAdminRequest?.Address;
            user.RoleId = updateUserByAdminRequest.RoleId;
            user.IsActive = updateUserByAdminRequest.IsActive;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
        ?? throw new KeyNotFoundException("User not found");

            // Soft delete
            user.IsActive = false;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<AdminUserPaginatedResponseDTO> GetUsersPagedAsync(int page, int pageSize)
        {
            var (users, totalCount) = await _userRepository.GetUsersWithRolesPagedAsync(page, pageSize);

            return new AdminUserPaginatedResponseDTO
            {
                Items = users.Select(x => new AdminUserResponseDTO
                {
                    UserId = x.UserId,
                    Username = x.Username,
                    FullName = x.FullName,
                    Email = x.Email,
                    Phone = x.Phone,
                    Address = x.Address,
                    RoleName = x.Role?.RoleName ?? "",
                    IsActive = x.IsActive,
                    CreateDate = x.CreateDate,
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
