using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Council
{
    public class CouncilReadDto
    {
        public int Id { get; set; }
        public int MajorId { get; set; }
        public string? MajorName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
