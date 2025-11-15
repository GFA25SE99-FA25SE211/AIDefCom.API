using System;
using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.Transcript
{
    public class TranscriptCreateDto
    {
        [Required(ErrorMessage = "SessionId is required.")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Transcript text is required.")]
        public string TranscriptText { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = false;
    }
}
