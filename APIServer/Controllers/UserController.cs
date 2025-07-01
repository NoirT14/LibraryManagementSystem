using APIServer.DTO.User;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return BadRequest("Không thể xác định thông tin user hiện tại");
            }

            var profile = await _userService.GetUserProfileAsync(currentUserId);
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateRequestDTO userProfileUpdateRequest)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return BadRequest("Không thể xác định thông tin user hiện tại");
            }

            await _userService.UpdateProfileAsync(currentUserId, userProfileUpdateRequest);
            return NoContent();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return BadRequest("Không thể xác định thông tin user hiện tại");
            }

            await _userService.ChangePasswordAsync(currentUserId, changePasswordRequest);
            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAllUserAsync();
            return Ok(users);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("admin/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDTO createUserRequest)
        {
            var userCreate = await _userService.CreateUserAsync(createUserRequest);
            return Ok(userCreate);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] AdminUpdateUserRequestDTO adminUpdateUserRequest)
        {
            await _userService.UpdateUserByAdminAsync(adminUpdateUserRequest);
            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("admin/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordRequestDTO resetPasswordRequest)
        {
            await _userService.ResetPasswordAsync(resetPasswordRequest);
            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("admin/delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }
    }
}
