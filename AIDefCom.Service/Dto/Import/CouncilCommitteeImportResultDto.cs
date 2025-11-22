using Microsoft.AspNetCore.Http;

namespace AIDefCom.Service.Dto.Import
{
    public class CouncilCommitteeImportResultDto : ImportResultDto
    {
        public List<int> CreatedCouncilIds { get; set; } = new();
        public List<string> CreatedCommitteeAssignmentIds { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
