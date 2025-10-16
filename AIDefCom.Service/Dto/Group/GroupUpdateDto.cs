using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Group
{
    public class GroupUpdateDto
    {
        public string ProjectCode { get; set; } = string.Empty;
        public string TopicTitle_EN { get; set; } = string.Empty;
        public string TopicTitle_VN { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
