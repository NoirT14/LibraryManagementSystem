using APIServer.DTO.User;

namespace APIServer.Service.Interfaces
{
    public interface IUserService
    {
        //User
        Task<UserProfileResponseDTO> GetUserProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UserProfileUpdateRequestDTO userProfileUpdateRequest);
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDTO changePasswordRequest);

        //Admin
        Task<List<AdminUserResponseDTO>> GetAllUserAsync();
        Task<AdminUserResponseDTO> CreateUserAsync(CreateUserRequestDTO createUserRequest);
        Task UpdateUserByAdminAsync(AdminUpdateUserRequestDTO updateUserByAdminRequest);
        Task ResetPasswordAsync(AdminResetPasswordRequestDTO resetPasswordRequest);
        Task DeleteUserAsync(int userId);
        Task<AdminUserPaginatedResponseDTO> GetUsersPagedAsync(int page, int pageSize);
    }
}
