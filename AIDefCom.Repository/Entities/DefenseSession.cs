using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class DefenseSession
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to Group
        public string GroupId { get; set; } = string.Empty;
        public Group? Group { get; set; }

        public string Location { get; set; } = string.Empty;
        public DateTime DefenseDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Foreign Key to Council
        public int CouncilId { get; set; }
        public Council? Council { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}