using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Services.EmailService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("emails")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService emailService;

        public EmailsController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpPost("otp")]
        public async Task<IActionResult> SendOtp()
        {
            try
            {
                var result = await emailService.GenerateAndSendOTP(HttpContext);
                return Ok(new { message = "OTP sent successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to send OTP.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail([FromQuery] string userId, [FromQuery] string senderId, [FromQuery] string content)
        {
            try
            {
                await emailService.SendEmail(userId, senderId, content);
                return Ok(new { message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to send email.", error = ex.Message });
            }
        }

        [HttpPost("otp/verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OTPVerificationRequest request)
        {
            try
            {
                var result = await emailService.VerifyOTP(request);
                return Ok(new { message = "OTP verified successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to verify OTP.", error = ex.Message });
            }
        }
    }
}
