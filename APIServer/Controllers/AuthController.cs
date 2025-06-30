using APIServer.DTO.Auth;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _authService.Authenticate(request);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (await _authService.IsUsernameTaken(request.Username))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    errorMessage = "Tên đăng nhập đã tồn tại"
                });
            }

            var emailExists = await _authService.IsEmailTaken(request.Email);
            if (emailExists)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    errorMessage = "Email đã được sử dụng"
                });
            }
            var result = await _authService.Register(request);
            if(!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            var result = await _authService.ResetPassword(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Password reset successfully!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            var result = await _authService.SendResetPasswordTokenAsync(request);

            return Ok(new { message = "If the email exists, a reset password link has been sent." });
        }
    }
}
