using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Student
    {
        public string Id { get; set; } = string.Empty; // Primary Key

        // Foreign Key to AppUser
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        // Foreign Key to Group
        public string GroupId { get; set; }
        public Group? Group { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Role { get; set; }
    }
}