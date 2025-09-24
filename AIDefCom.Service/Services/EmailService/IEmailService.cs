using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AIDefCom.Service.Dto.Account;
using Microsoft.AspNetCore.Http;


namespace AIDefCom.Service.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(MessageOTP message);
        Task SendEmail(string userId, string senderId, string content);
        Task<string> GenerateAndSendOTP(HttpContext httpContext);
        Task<string> VerifyOTP(OTPVerificationRequest request);
    }
}
