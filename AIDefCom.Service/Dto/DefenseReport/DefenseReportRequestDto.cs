using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.DefenseReport
{
    public class DefenseReportRequestDto
    {
        [Required(ErrorMessage = "Transcript ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Transcript ID must be greater than 0")]
        public int TranscriptId { get; set; }
    }
}
