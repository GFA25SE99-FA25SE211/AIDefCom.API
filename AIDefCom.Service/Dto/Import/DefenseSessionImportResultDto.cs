using Microsoft.AspNetCore.Http;

namespace AIDefCom.Service.Dto.Import
{
    public class DefenseSessionImportResultDto : ImportResultDto
    {
        public List<int> CreatedDefenseSessionIds { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
