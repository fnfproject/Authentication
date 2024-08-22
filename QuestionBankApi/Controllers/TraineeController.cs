
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.Dtos;
using QuestionBankApi.Models;
using QuestionBankApi.Services.cs;

namespace QuestionBankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("cors")]
    public class TraineeController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;

        public TraineeController(JwtService jwtService, IUserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userService.RegisterUser(model,UserRole.Trainee);
            if (user == null)
                return Conflict("Username or Email already taken.");

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login loginDto)
        {
            var user = _userService.AuthenticateUser(loginDto.Username, loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Check if the user has enabled 2FA
            if (!user.Is2FAEnabled)
            {
                // Check if the user has completed 2FA verification
                if (!user.IsVerified)
                {
                    return Unauthorized("User is not verified. Please complete 2FA verification.");
                }
            }

            // Generate a token only if the user is authenticated and 2FA is verified (if applicable)
            var token = _jwtService.GenerateToken(user.Username, user.Is2FAEnabled);
            return Ok(new { Token = token });
        }

        [HttpPost("enable-2fa")]
        public IActionResult Enable2FA([FromBody] Enable2FADto model)
        {
            var (qrCode, _) = _userService.EnableTwoFactorAuth(model.Username);
            if (qrCode == null) return NotFound("User not found.");

            return File(qrCode, "image/png");
        }

        [HttpPost("verify-2fa")]
        public IActionResult Verify2FA([FromBody] Verify2FADto model)
        {
            // Verify the 2FA code
            var isValid = _userService.VerifyTwoFactorCode(model.Username, model.Code);

            if (!isValid)
            {
                return Unauthorized("Invalid 2FA code.");
            }

            // Update the user's verification status in the UserService
            var user = _userService.MarkUserAsVerified(model.Username);

            // Generate a JWT token after successful verification
            var token = _jwtService.GenerateToken(user.Username, user.Is2FAEnabled);

            // Return a success message along with the token
            return Ok(new
            {
                Message = "2FA verification successful.",
                Token = token
            });
        }
    }
}
