using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Account
{
    public class OTPVerificationRequest
    {
        public string Email { get; set; }
        public string OTP { get; set; }
    }
}
