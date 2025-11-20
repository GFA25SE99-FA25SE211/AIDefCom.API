using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.CommitteeAssignment
{
    public class CommitteeAssignmentCreateDto
    {
        public string LecturerId { get; set; } = string.Empty;
        public int CouncilId { get; set; }
        public int CouncilRoleId { get; set; }
    }
}
