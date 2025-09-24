using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Account
{
    public class SetPasswordDTO
    {
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
