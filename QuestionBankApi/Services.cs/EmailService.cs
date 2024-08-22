using System.Net.Mail;
using System.Net;

namespace QuestionBankApi.Services.cs
{
    public class EmailService
    {
        private const string AdminEmail = "prajwal"; // Hardcoded admin email

        // Method to send an OTP to the admin
        public void SendOtpToAdmin(string otp)
        {
            var subject = "New Trainer OTP Verification";
            var body = $"A new trainer has requested access. The OTP for verification is: {otp}";

            SendEmail(AdminEmail, subject, body);
        }

        // Method to notify admin about new trainer registration
        public void NotifyAdminAboutRegistration(string email)
        {
            var subject = "New Trainer Registration";
            var body = $"A new trainer has registered with the email: {email}. Please review the registration.";

            SendEmail(AdminEmail, subject, body);
        }

        private void SendEmail(string to, string subject, string body)
        {
            // Replace these with actual SMTP settings
            var smtpClient = new SmtpClient("smtp.example.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("username@example.com", "password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@example.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };

            mailMessage.To.Add(to);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Handle any exceptions related to email sending
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
