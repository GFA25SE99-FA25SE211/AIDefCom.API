using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class CommitteeAssignment
    {
        public string Id { get; set; } = string.Empty; // Primary Key

        // Foreign Key to Lecturer
        public string LecturerId { get; set; } = string.Empty;
        public Lecturer? Lecturer { get; set; }

        // Foreign Key to Council
        public int CouncilId { get; set; }
        public Council? Council { get; set; }

        // Foreign Key to CouncilRole
        public int CouncilRoleId { get; set; }
        public CouncilRole? CouncilRole { get; set; }
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
    }
}