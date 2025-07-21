using APIServer.DTO.User;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{userId}/profile")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var profile = await _userService.GetUserProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("{userId}/profile")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UserProfileUpdateRequestDTO userProfileUpdateRequest)
        {
            await _userService.UpdateProfileAsync(userId, userProfileUpdateRequest);
            return NoContent();
        }

        [HttpPut("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            await _userService.ChangePasswordAsync(userId, changePasswordRequest);
            return NoContent();
        }

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAllUserAsync();
            return Ok(users);
        }

        [HttpPost("admin/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDTO createUserRequest)
        {
            var userCreate = await _userService.CreateUserAsync(createUserRequest);
            return Ok(userCreate);
        }

        [HttpPut("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] AdminUpdateUserRequestDTO adminUpdateUserRequest)
        {
            await _userService.UpdateUserByAdminAsync(adminUpdateUserRequest);
            return NoContent();
        }

        [HttpPost("admin/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordRequestDTO resetPasswordRequest)
        {
            await _userService.ResetPasswordAsync(resetPasswordRequest);
            return NoContent();
        }

        [HttpDelete("admin/delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }
    }
}
