using AIDefCom.Repository.Entities;
using AIDefCom.Service.Dto.Account;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using OtpNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.EmailService
{
    public class EmailService(EmailConfiguration _emailConfig, UserManager<AppUser> _userManager, ConcurrentDictionary<string, OtpEntry> _otpStorage) : IEmailService
    {
        public async Task<string> GenerateAndSendOTP(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
                throw new UnauthorizedAccessException("Missing Authorization header");

            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Invalid Authorization header format");

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Invalid JWT token: " + ex.Message);
            }

            var email = jwtToken.Claims.ElementAt(2)?.Value;
            //jwtToken có danh sách, claims email nằm ở thứ 2 danh sách.
            if (string.IsNullOrEmpty(email))
                throw new Exception("Email claim not found in token");

            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secretKey);
            var totp = new Totp(secretKey);
            var otp = totp.ComputeTotp(DateTime.UtcNow);

            _otpStorage[email] = new OtpEntry
            {
                Base32Secret = base32Secret,
                TimeStamp = DateTime.UtcNow
            };

            var message = new MessageOTP(
                new string[] { email },
                "Your One-Time Password (OTP)",
                $@"
                <h1>OTP Verification</h1>
                <p>Dear {email},</p>
                <p>Use the following OTP: <strong>{otp}</strong></p>
                <p>It is valid for 10 minutes.</p>"
            );

            SendEmail(message);
            return "OTP sent";
        }

        public void SendEmail(MessageOTP message)
        {
            try
            {
                var emailMessage = CreateEmailMessage(message);
                Send(emailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Lỗi khi gửi email: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmail(string userId, string senderId, string content)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var sender = await _userManager.FindByIdAsync(senderId);

            if (user == null || sender == null || string.IsNullOrEmpty(user.Email))
            {
                throw new Exception("User, sender not found or user email is empty");
            }

            var message = new MessageOTP(new List<string> { user.Email }, "Notification", content);
            SendEmail(message);
        }

        private MimeMessage CreateEmailMessage(MessageOTP message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To.ConvertAll(to => new MailboxAddress(to, to)));
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message.Content
            };
            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.StartTls);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] SMTP error: {ex.Message}");
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public async Task<string> VerifyOTP(OTPVerificationRequest request)
        {
            if (!_otpStorage.TryGetValue(request.Email, out var otpEntry))
                throw new KeyNotFoundException("OTP request not found");

            TimeSpan expirationTime = TimeSpan.FromMinutes(10);
            if (DateTime.UtcNow - otpEntry.TimeStamp > expirationTime)
            {
                _otpStorage.TryRemove(request.Email, out _);
                throw new UnauthorizedAccessException("OTP has expired");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var secretKey = Base32Encoding.ToBytes(otpEntry.Base32Secret);
            var totp = new Totp(secretKey);
            bool isValid = totp.VerifyTotp(request.OTP, out long timeStepMatched, new VerificationWindow(2, 2));

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid OTP");

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return "Email confirmed successfully";
        }
    }
}
