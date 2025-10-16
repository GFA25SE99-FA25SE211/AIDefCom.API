using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Student
{
    public class StudentCreateDto
    {
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Role { get; set; }
    }
}
