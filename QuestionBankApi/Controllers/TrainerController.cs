using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.Dtos;
using QuestionBankApi.Models;
using QuestionBankApi.Services.cs;

namespace QuestionBankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainerController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public TrainerController(JwtService jwtService, IUserService userService, OtpService otpService, EmailService emailService)
        {
            _jwtService = jwtService;
            _userService = userService;
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Register the trainer with 'Trainer' role and default 'IsAdminApproved' as false
            var user = _userService.RegisterUser(model, UserRole.Trainer);

            if (user == null)
                return Conflict("Username or Email already taken.");

            // Notify admin about new trainer registration
            _emailService.NotifyAdminAboutRegistration(user.Email);

            return Ok("Trainer registered successfully. Admin approval required.");
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
                if (!user.IsVerified)
                {
                    return Unauthorized("User is not verified. Please complete 2FA verification.");
                }
            }

            if (!user.IsAdminApproved)
            {
                return Unauthorized("Trainer not approved by admin.");
            }

            var token = _jwtService.GenerateToken(user.Username, user.Is2FAEnabled);
            return Ok(new { Token = token });
        }

        [HttpPost("enable-2fa")]
        public IActionResult Enable2FA([FromBody] Enable2FADto model)
        {
            var (qrCode, _) = _userService.EnableTwoFactorAuth(model.Username);
            if (qrCode == null) return NotFound("Trainer not found.");

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

            // Update the trainer's verification status in the UserService
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

        [HttpPost("send-admin-otp")]
         public IActionResult SendAdminOtp([FromBody] SendAdminOtpDto model)
        {
            var otp = _otpService.GenerateOtp(); // Implement OTP generation logic in your OTP service
            _emailService.SendOtpToAdmin(otp); // Implement email sending logic in your EmailService
            return Ok("OTP sent to admin.");
        }

        [HttpPost("verify-admin-otp")]
        public IActionResult VerifyAdminOtp([FromBody] VerifyAdminOtpDto model)
        {
            var isValid = _otpService.ValidateOtp(model.Otp); // Implement OTP validation logic in your OTP service

            if (!isValid)
            {
                return Unauthorized("Invalid OTP.");
            }

            return Ok("OTP verified successfully.");
        }
    }
}
