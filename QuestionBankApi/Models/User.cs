using System.ComponentModel.DataAnnotations;

namespace QuestionBankApi.Models
{

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? TwoFactorSecretKey { get; set; }
        public bool Is2FAEnabled { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAdminApproved { get; set; } // New column
        public UserRole Role { get; set; } // New column for user role
    }

    public enum UserRole
    {
        Trainee,
        Trainer,
        Admin
    }

}
