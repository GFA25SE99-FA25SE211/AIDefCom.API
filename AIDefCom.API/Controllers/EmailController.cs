using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Services.EmailService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for email operations
    /// </summary>
    [Route("api/emails")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailsController> _logger;

        public EmailsController(IEmailService emailService, ILogger<EmailsController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Send OTP to user email
        /// </summary>
        [HttpPost("otp")]
        public async Task<IActionResult> SendOtp()
        {
            _logger.LogInformation("Sending OTP");
            var result = await _emailService.GenerateAndSendOTP(HttpContext);
            
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Email_Success0001,
                Message = SystemMessages.Email_Success0001,
                Data = result
            });
        }

        /// <summary>
        /// Send email to a user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendEmail([FromQuery] string userId, [FromQuery] string senderId, [FromQuery] string content)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException(nameof(userId), "UserId, SenderId, and Content are required");
            }

            _logger.LogInformation("Sending email from {SenderId} to {UserId}", senderId, userId);
            await _emailService.SendEmail(userId, senderId, content);
            
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Email_Success0002,
                Message = SystemMessages.Email_Success0002
            });
        }

        /// <summary>
        /// Verify OTP
        /// </summary>
        [HttpPost("otp/verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OTPVerificationRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "OTP verification request is required");
            }

            _logger.LogInformation("Verifying OTP for email: {Email}", request.Email);
            var result = await _emailService.VerifyOTP(request);
            
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Email_Success0003,
                Message = SystemMessages.Email_Success0003,
                Data = result
            });
        }
    }
}
