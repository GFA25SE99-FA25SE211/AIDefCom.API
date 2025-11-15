using System;

namespace AIDefCom.Service.Dto.Transcript
{
    public class TranscriptReadDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string TranscriptText { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
