using System;

namespace AIDefCom.Repository.Entities
{
    public class Recording
    {
        public Guid Id { get; set; } // Primary Key
        public string BlobPath { get; set; } = string.Empty;
        public string BlobUrl { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public int? DurationSeconds { get; set; }
        public long? SizeBytes { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string? Notes { get; set; }

        // Ownership
        public string UserId { get; set; } = default!;
        public AppUser? User { get; set; }

        // Foreign Key to Transcript (Transcript.Id is int)
        public int? TranscriptId { get; set; }
        public Transcript? Transcript { get; set; }
    }
}
