using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class CommitteeAssignment
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to AppUser
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        // Foreign Key to Council
        public int CouncilId { get; set; }
        public Council? Council { get; set; }

        // Foreign Key to DefenseSession
        public int SessionId { get; set; }
        public DefenseSession? Session { get; set; }

        public string Role { get; set; } = string.Empty; // Chair, Member, Secretary
    }
}