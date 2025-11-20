using Microsoft.AspNetCore.Http;

namespace AIDefCom.Service.Dto.Import
{
    public class StudentGroupImportRequestDto
    {
        public int SemesterId { get; set; }
        public int MajorId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
