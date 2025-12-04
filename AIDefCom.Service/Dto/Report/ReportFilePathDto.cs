using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Report
{
    public class ReportFilePathDto
    {
        public int ReportId { get; set; }
        public string? FilePath { get; set; }
        public int SessionId { get; set; }
        public string SessionLocation { get; set; } = string.Empty;
        public DateTime DefenseDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SessionStatus { get; set; } = string.Empty;
        public string? GroupId { get; set; }
        public int? CouncilId { get; set; }
    }
}
