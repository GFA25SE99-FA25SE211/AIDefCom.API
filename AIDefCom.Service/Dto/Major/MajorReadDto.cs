using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Major
{
    public class MajorReadDto
    {
        public int Id { get; set; }
        public string MajorName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
