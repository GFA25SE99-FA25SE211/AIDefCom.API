using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.DefenseSession
{
    public class DefenseSessionCreateDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime DefenseDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CouncilId { get; set; }

    }
}
