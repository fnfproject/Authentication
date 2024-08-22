using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.Dtos;
using QuestionBankApi.Models;
using QuestionBankApi.Services.cs;

namespace QuestionBankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;

        // Hardcoded admin credentials
        private const string AdminUsername = "admin";
        private const string AdminPassword = "Admin@123";

        public AdminController(JwtService jwtService, IUserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto model)
        {
            // For admins only: registration is restricted, so we send a contact message.
            return BadRequest("Contact higher authority for admin registration.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login loginDto)
        {
            // Check hardcoded credentials for admin login
            if (loginDto.Username == AdminUsername && loginDto.Password == AdminPassword)
            {
                // Generate a token and set the role as Admin
                var token = _jwtService.GenerateToken(AdminUsername, false);

                // Add admin to the user database if not already present
                var adminUser = _userService.GetUserByUsername(AdminUsername);
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        Username = AdminUsername,
                        Email = "admin@example.com", // This can be hardcoded or configured
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword), // Hash the hardcoded password
                        Role = UserRole.Admin, // Set role as Admin
                        IsAdminApproved = true, // Admin is pre-approved
                        Is2FAEnabled = false,
                        IsVerified = true // Admin is verified by default
                    };

                    _userService.AddUser(adminUser);
                }

                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid admin credentials.");
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            // Check if the username is for admin
            if (model.Username != AdminUsername)
            {
                return Unauthorized("Only admin can reset the password.");
            }

            // Here, you could implement logic to send a password reset email to the admin.
            // For now, we just return a message.
            return Ok("Password reset functionality for admin is not implemented yet. Please contact support.");
        }


    }
}



