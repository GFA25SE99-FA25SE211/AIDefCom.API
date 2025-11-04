using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class StudentGroup
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to Student (User_id in diagram)
        public string UserId { get; set; } = string.Empty;
        public Student? Student { get; set; }

        // Foreign Key to Group
        public string GroupId { get; set; } = string.Empty;
        public Group? Group { get; set; }

        public string? GroupRole { get; set; } // e.g., Leader, Member
    }
}
