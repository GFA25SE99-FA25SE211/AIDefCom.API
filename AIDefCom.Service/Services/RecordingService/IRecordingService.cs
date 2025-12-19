using System;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.RecordingService
{
    public interface IRecordingService
    {
        Task<(Guid recordingId, Uri uploadUri, string blobUrl, string blobPath)> BeginUploadAsync(string userId, string mimeType);
        Task FinalizeAsync(Guid recordingId, int durationSec, long sizeBytes, string? notes, int transcriptId);
        Task<Uri> GetReadSasAsync(Guid recordingId, TimeSpan ttl);
        Task<RecordingDto?> GetRecordingByReportIdAsync(int reportId);
    }

    public class RecordingDto
    {
        public Guid Id { get; set; }
        public string BlobPath { get; set; } = string.Empty;
        public string BlobUrl { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public int? DurationSeconds { get; set; }
        public long? SizeBytes { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string? Notes { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public int? TranscriptId { get; set; }
    }
}
