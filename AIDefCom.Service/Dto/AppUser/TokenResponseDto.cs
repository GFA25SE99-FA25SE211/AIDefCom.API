using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.AppUser
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool HasPassword { get; set; }
        public bool IsNewUser { get; set; }
    }
}
