using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.CommitteeAssignment
{
    public class CommitteeAssignmentReadDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int CouncilId { get; set; }
        public int SessionId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
