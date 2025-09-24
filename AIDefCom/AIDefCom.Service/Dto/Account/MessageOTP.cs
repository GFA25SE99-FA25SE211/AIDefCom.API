using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Account
{
    public class MessageOTP
    {
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public MessageOTP(IEnumerable<string> to, string subject, string content)
        {
            To = new List<string>(to);
            Subject = subject;
            Content = content;
        }
    }
}
