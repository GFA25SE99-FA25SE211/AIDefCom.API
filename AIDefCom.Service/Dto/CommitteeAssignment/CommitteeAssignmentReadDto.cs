using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.CommitteeAssignment
{
    public class CommitteeAssignmentReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string LecturerId { get; set; } = string.Empty;
        public string? LecturerName { get; set; }
        public int CouncilId { get; set; }
        public int CouncilRoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
