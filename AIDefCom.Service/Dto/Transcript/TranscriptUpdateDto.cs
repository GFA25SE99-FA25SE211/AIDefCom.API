using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.Transcript
{
    public class TranscriptUpdateDto
    {
        [Required(ErrorMessage = "Transcript text is required.")]
        public string TranscriptText { get; set; } = string.Empty;

        public bool IsApproved { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;
    }
}
