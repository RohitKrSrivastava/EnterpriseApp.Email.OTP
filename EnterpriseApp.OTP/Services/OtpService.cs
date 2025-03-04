using EnterpriseApp.Configuration;
using EnterpriseApp.OTP.Constants;
using EnterpriseApp.OTP.Models;
using EnterpriseApp.OTP.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;


namespace EnterpriseApp.OTP.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<OtpService> _logger;
        private readonly Random _random;

        public OtpService(
            IMemoryCache cache,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings,
            ILogger<OtpService> logger)
        {
            _cache = cache;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _random = new Random();
        }

        public string GenerateOtp()
        {
            return _random.Next(100000, 999999).ToString();
        }

        public async Task<string> GenerateAndSendOtpAsync(string userEmail)
        {
            if (!IsValidEmail(userEmail))
            {
                return OtpConstants.STATUS_EMAIL_INVALID;
            }

            if (!userEmail.EndsWith(_emailSettings.AllowedDomain, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"Email with non-allowed domain attempted: {userEmail}");
                return OtpConstants.STATUS_EMAIL_INVALID;
            }

            string otp = GenerateOtp();

            // Store OTP in cache with expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(OtpConstants.OTP_VALID_MINUTES));

            string cacheKey = $"OTP_{userEmail}";

            var otpData = new OtpData
            {
                Code = otp,
                RemainingAttempts = OtpConstants.MAX_OTP_ATTEMPTS,
                ExpiryTime = DateTime.UtcNow.AddMinutes(OtpConstants.OTP_VALID_MINUTES)
            };

            _cache.Set(cacheKey, otpData, cacheEntryOptions);

            string emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333;'>Your One-Time Password</h2>
                        <p>Your OTP Code is:</p>
                        <div style='background-color: #fff; padding: 15px; border-radius: 5px; font-size: 24px; font-weight: bold; text-align: center; letter-spacing: 5px;'>
                            {otp}
                        </div>
                        <p style='margin-top: 20px;'>The code is valid for {OtpConstants.OTP_VALID_MINUTES} minute.</p>
                        <p style='font-size: 12px; color: #888; margin-top: 30px;'>If you did not request this code, please ignore this email.</p>
                    </div>
                </body>
                </html>";

            try
            {
                bool emailSent = await _emailService.SendEmailAsync(userEmail, "Your OTP Code", emailBody);
                if (!emailSent)
                {
                    _logger.LogError($"Failed to send email to {userEmail}");
                    return OtpConstants.STATUS_EMAIL_FAIL;
                }

                _logger.LogInformation($"OTP sent successfully to {userEmail}");
                return OtpConstants.STATUS_EMAIL_OK;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception when sending OTP to {userEmail}: {ex.Message}");
                return OtpConstants.STATUS_EMAIL_FAIL;
            }
        }

        public async Task<string> VerifyOtpAsync(string userEmail, string otp)
        {
            string cacheKey = $"OTP_{userEmail}";

            if (!_cache.TryGetValue(cacheKey, out OtpData otpData))
            {
                _logger.LogWarning($"OTP verification attempted for non-existent or expired OTP: {userEmail}");
                return OtpConstants.STATUS_OTP_TIMEOUT;
            }

            if (DateTime.UtcNow > otpData.ExpiryTime)
            {
                _cache.Remove(cacheKey);
                _logger.LogInformation($"OTP expired for {userEmail}");
                return OtpConstants.STATUS_OTP_TIMEOUT;
            }

            if (otp != otpData.Code)
            {
                otpData.RemainingAttempts--;

                if (otpData.RemainingAttempts <= 0)
                {
                    _cache.Remove(cacheKey);
                    _logger.LogWarning($"Maximum OTP attempts exceeded for {userEmail}");
                    return OtpConstants.STATUS_OTP_FAIL;
                }

                _cache.Set(cacheKey, otpData, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(otpData.ExpiryTime - DateTime.UtcNow));

                _logger.LogInformation($"Invalid OTP attempt for {userEmail}. Remaining attempts: {otpData.RemainingAttempts}");
                return $"Invalid OTP. Remaining attempts: {otpData.RemainingAttempts}";
            }

            _cache.Remove(cacheKey);
            _logger.LogInformation($"OTP verified successfully for {userEmail}");
            return OtpConstants.STATUS_OTP_OK;
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
