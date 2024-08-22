using System;
using System.Security.Cryptography;

namespace QuestionBankApi.Services.cs
{
    public class OtpService
    {
        private static readonly int OtpLength = 6; // Length of the OTP
        private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(5); // Expiry time for OTP

        // In-memory store for OTPs (for demonstration; consider using a database or cache in production)
        private static readonly Dictionary<string, (string Otp, DateTime CreatedAt)> OtpStore = new();

        public string GenerateOtp()
        {
            // Generate a random OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store the OTP with its creation time
            OtpStore[otp] = (Otp: otp, CreatedAt: DateTime.UtcNow);

            return otp;
        }

        public bool ValidateOtp(string otp)
        {
            if (OtpStore.TryGetValue(otp, out var otpData))
            {
                // Check if the OTP has expired
                if (DateTime.UtcNow - otpData.CreatedAt <= OtpExpiry)
                {
                    // OTP is valid and within expiry time
                    OtpStore.Remove(otp); // Remove OTP after validation
                    return true;
                }
            }

            // OTP is invalid or expired
            return false;
        }
    }
}
