using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Report
{
    public class ReportCreateDto
    {
        public int SessionId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? SummaryText { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
