using QuestionBankApi.Dtos;
using QuestionBankApi.Models;

namespace QuestionBankApi.Services.cs
{
    public interface IUserService
    {
        User RegisterUser(RegisterDto model,UserRole role);
        User AuthenticateUser(string username, string password);
        (byte[] QrCode, string SecretKey) EnableTwoFactorAuth(string username);
        bool VerifyTwoFactorCode(string username, string code);

        User MarkUserAsVerified(string username);

        void ApprovedTrainer(string username);

        User GetUserByUsername(string username);

        void AddUser(User user);
    }
}
