using System.Collections.Generic;

namespace AIDefCom.Service.Dto.Import
{
    public class StudentGroupImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
        public List<string> CreatedStudentIds { get; set; } = new();
        public List<string> CreatedGroupIds { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
