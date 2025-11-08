using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.TranscriptAnalysis
{
    public class TranscriptAnalysisRequestDto
    {
        [Required(ErrorMessage = "Transcript text is required.")]
        public string Transcript { get; set; } = string.Empty;
        public int DefenseSessionId { get; set; }
    }
}
