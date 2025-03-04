namespace EnterpriseApp.OTP.Services.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<string> GenerateAndSendOtpAsync(string userEmail);
        Task<string> VerifyOtpAsync(string userEmail, string otp);
        bool IsValidEmail(string email);
    }
}