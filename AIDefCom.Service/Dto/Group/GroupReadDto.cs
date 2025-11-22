using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Group
{
    public class GroupReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string TopicTitle_EN { get; set; } = string.Empty;
        public string TopicTitle_VN { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public int MajorId { get; set; }
        public string? MajorName { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? TotalScore { get; set; }
    }
}
