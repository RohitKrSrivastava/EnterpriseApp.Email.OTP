using EnterpriseApp.Configuration;
using EnterpriseApp.OTP.Constants;
using EnterpriseApp.OTP.Models;
using EnterpriseApp.OTP.Services;
using EnterpriseApp.OTP.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnterpriseApp.Tests
{
    public class OtpServiceTests
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
        private readonly Mock<ILogger<OtpService>> _loggerMock;
        private readonly OtpService _otpService;

        public OtpServiceTests()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _cacheEntryMock = new Mock<ICacheEntry>();
            _emailServiceMock = new Mock<IEmailService>();
            _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
            _loggerMock = new Mock<ILogger<OtpService>>();

            _emailSettingsMock.Setup(x => x.Value).Returns(new EmailSettings
            {
                AllowedDomain = ".dso.org.sg"
            });

            _cacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(_cacheEntryMock.Object);

            _otpService = new OtpService(
                _cacheMock.Object,
                _emailServiceMock.Object,
                _emailSettingsMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void GenerateOtp_ReturnsCorrectLength()
        {
            // Act
            var otp = _otpService.GenerateOtp();

            // Assert
            Assert.Equal(OtpConstants.OTP_LENGTH, otp.Length);
            Assert.True(int.TryParse(otp, out _));
        }

        [Theory]
        [InlineData("test@dso.org.sg", true)]
        [InlineData("test@example.com", false)]
        [InlineData("invalid-email", false)]
        public void IsValidEmail_ValidatesCorrectly(string email, bool expectedResult)
        {
            // Act
            var result = _otpService.IsValidEmail(email);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GenerateAndSendOtpAsync_WithInvalidEmail_ReturnsInvalidStatus()
        {
            // Arrange
            string invalidEmail = "invalid-email";

            // Act
            var result = await _otpService.GenerateAndSendOtpAsync(invalidEmail);

            // Assert
            Assert.Equal(OtpConstants.STATUS_EMAIL_INVALID, result);
        }

        [Fact]
        public async Task GenerateAndSendOtpAsync_WithNonAllowedDomain_ReturnsInvalidStatus()
        {
            // Arrange
            string nonAllowedEmail = "test@example.com";

            // Act
            var result = await _otpService.GenerateAndSendOtpAsync(nonAllowedEmail);

            // Assert
            Assert.Equal(OtpConstants.STATUS_EMAIL_INVALID, result);
        }

        [Fact]
        public async Task GenerateAndSendOtpAsync_WithValidEmail_SendsEmailAndReturnsOkStatus()
        {
            // Arrange
            string validEmail = "test@dso.org.sg";
            _emailServiceMock
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _otpService.GenerateAndSendOtpAsync(validEmail);

            // Assert
            Assert.Equal(OtpConstants.STATUS_EMAIL_OK, result);
            _emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    It.Is<string>(email => email == validEmail),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}