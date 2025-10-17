using System;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.RecordingService
{
    public interface IRecordingService
    {
        Task<(Guid recordingId, Uri uploadUri, string blobUrl, string blobPath)> BeginUploadAsync(string userId, string mimeType);
        Task FinalizeAsync(Guid recordingId, int durationSec, long sizeBytes, string? notes);
        Task<Uri> GetReadSasAsync(Guid recordingId, TimeSpan ttl);
    }
}
