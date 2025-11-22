using Microsoft.AspNetCore.Http;

namespace AIDefCom.Service.Dto.Import
{
    public class CouncilCommitteeImportRequestDto
    {
        public int MajorId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
