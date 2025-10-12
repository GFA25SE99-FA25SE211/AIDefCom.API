using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Report
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to DefenseSession
        public int SessionId { get; set; }
        public DefenseSession? Session { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string? SummaryText { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}