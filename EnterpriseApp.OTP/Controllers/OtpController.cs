using EnterpriseApp.OTP.Constants;
using EnterpriseApp.OTP.Models;
using EnterpriseApp.OTP.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EnterpriseApp.OTP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly ILogger<OtpController> _logger;

        public OtpController(IOtpService otpService, ILogger<OtpController> logger)
        {
            _otpService = otpService;
            _logger = logger;
        }

        /// <summary>
        /// Generates and sends an OTP to the specified email address
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateOtp([FromBody] EmailRequest request)
        {
            _logger.LogInformation($"OTP generation requested for email: {request.Email}");

            var result = await _otpService.GenerateAndSendOtpAsync(request.Email);

            if (result == OtpConstants.STATUS_EMAIL_OK)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = result,
                    Data = null
                });
            }

            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = result,
                Data = null
            });
        }

        /// <summary>
        /// Verifies the OTP entered by the user
        /// </summary>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            _logger.LogInformation($"OTP verification requested for email: {request.Email}");

            var result = await _otpService.VerifyOtpAsync(request.Email, request.Otp);

            if (result == OtpConstants.STATUS_OTP_OK)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = result,
                    Data = null
                });
            }

            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = result,
                Data = null
            });
        }
    }
}
