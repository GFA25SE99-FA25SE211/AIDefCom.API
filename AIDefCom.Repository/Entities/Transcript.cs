using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Transcript
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to DefenseSession
        public int SessionId { get; set; }
        public DefenseSession? Session { get; set; }

        public string TranscriptText { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}