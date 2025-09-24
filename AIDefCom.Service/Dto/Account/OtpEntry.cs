using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Account
{
    public class OtpEntry
    {
        public string Base32Secret { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
