using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Council
{
    public class CouncilUpdateDto
    {
        public int MajorId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
