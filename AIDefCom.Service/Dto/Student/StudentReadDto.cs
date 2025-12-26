using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Student
{
    public class StudentReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? GroupId { get; set; }
        public string? GroupRole { get; set; } // Role của student trong group (Leader, Member)
    }
}
