using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class MemberNote
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to AppUser
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        // Foreign Key to Group
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string? NoteContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}