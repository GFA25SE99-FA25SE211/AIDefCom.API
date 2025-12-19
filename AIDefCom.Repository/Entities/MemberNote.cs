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

        // Foreign Key to CommitteeAssignment
        public string CommitteeAssignmentId { get; set; } = string.Empty;
        public CommitteeAssignment? CommitteeAssignment { get; set; }

        // Foreign Key to DefenseSession (changed from Group)
        public int SessionId { get; set; }
        public DefenseSession? Session { get; set; }

        public string? NoteContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
