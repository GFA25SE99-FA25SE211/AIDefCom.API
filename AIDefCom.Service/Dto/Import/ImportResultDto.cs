using System.Collections.Generic;

namespace AIDefCom.Service.Dto.Import
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
        public List<string> CreatedUserIds { get; set; } = new();
    }

    public class ImportErrorDto
    {
        public int Row { get; set; }
        public string Field { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
