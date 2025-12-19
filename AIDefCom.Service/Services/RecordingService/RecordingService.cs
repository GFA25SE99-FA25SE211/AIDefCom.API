using System;
using System.Threading.Tasks;
using AIDefCom.Repository.Entities;
using AIDefCom.Repository.Repositories.RecordingRepository;
using AIDefCom.Repository.UnitOfWork;

namespace AIDefCom.Service.Services.RecordingService
{
    public class RecordingService : IRecordingService
    {
        private readonly IRecordingRepository _recordings;
        private readonly IUnitOfWork _uow;
        private readonly RecordingStorageService _storage;

        public RecordingService(IRecordingRepository recordings, IUnitOfWork uow, RecordingStorageService storage)
        {
            _recordings = recordings;
            _uow = uow;
            _storage = storage;
        }

        public async Task<(Guid recordingId, Uri uploadUri, string blobUrl, string blobPath)> BeginUploadAsync(string userId, string mimeType)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(mimeType)) throw new ArgumentException("mimeType is required", nameof(mimeType));

            var ext = MimeToExt(mimeType);
            var (uploadUri, blobUrl, blobPath) = await _storage.CreateUploadSasAsync(userId, ext);

            var entity = new Recording
            {
                Id = Guid.NewGuid(),
                BlobPath = blobPath,
                BlobUrl = blobUrl,
                MimeType = mimeType,
                CreatedUtc = DateTime.UtcNow,
                UserId = userId
            };

            await _recordings.AddAsync(entity);
            await _uow.CompleteAsync();

            return (entity.Id, uploadUri, blobUrl, blobPath);
        }

        public async Task FinalizeAsync(Guid recordingId, int durationSec, long sizeBytes, string? notes, int transcriptId)
        {
            if (transcriptId <= 0) throw new ArgumentException("transcriptId must be positive", nameof(transcriptId));
            
            var rec = await _recordings.GetByIdAsync(recordingId) ?? throw new InvalidOperationException("Recording not found");
            rec.DurationSeconds = durationSec;
            rec.SizeBytes = sizeBytes;
            rec.Notes = notes;
            rec.TranscriptId = transcriptId;
            
            _recordings.Update(rec);
            await _uow.CompleteAsync();
        }

        public async Task<Uri> GetReadSasAsync(Guid recordingId, TimeSpan ttl)
        {
            var rec = await _recordings.GetByIdAsync(recordingId) ?? throw new InvalidOperationException("Recording not found");
            return await _storage.CreateReadSasAsync(rec.BlobPath, ttl);
        }

        public async Task<RecordingDto?> GetRecordingByReportIdAsync(int reportId)
        {
            var recording = await _recordings.GetByReportIdAsync(reportId);
            
            if (recording == null)
                return null;

            return new RecordingDto
            {
                Id = recording.Id,
                BlobPath = recording.BlobPath,
                BlobUrl = recording.BlobUrl,
                MimeType = recording.MimeType,
                DurationSeconds = recording.DurationSeconds,
                SizeBytes = recording.SizeBytes,
                CreatedUtc = recording.CreatedUtc,
                Notes = recording.Notes,
                UserId = recording.UserId,
                UserFullName = recording.User?.FullName,
                TranscriptId = recording.TranscriptId
            };
        }

        private static string MimeToExt(string mime)
        {
            // Minimal mapping; default to webm
            return mime?.ToLowerInvariant() switch
            {
                "audio/webm" => "webm",
                "audio/mpeg" => "mp3",
                "audio/mp4" => "m4a",
                "audio/wav" => "wav",
                "audio/x-wav" => "wav",
                "audio/ogg" => "ogg",
                _ => "webm"
            };
        }
    }
}
