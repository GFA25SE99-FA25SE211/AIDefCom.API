using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.CommitteeAssignment
{
    public class CommitteeAssignmentUpdateDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CouncilId { get; set; }
        public int SessionId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
