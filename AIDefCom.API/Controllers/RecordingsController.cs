using System;
using System.Threading.Tasks;
using AIDefCom.Service.Services.RecordingService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordingsController : ControllerBase
    {
        private readonly IRecordingService _recordingService;

        public RecordingsController(IRecordingService recordingService)
        {
            _recordingService = recordingService;
        }

        public record BeginUploadRequest(string UserId, string MimeType);
        public record BeginUploadResponse(Guid RecordingId, string UploadUrl, string BlobUrl, string BlobPath, string MimeType);

        [HttpPost("begin-upload")]
        public async Task<ActionResult<BeginUploadResponse>> BeginUpload([FromBody] BeginUploadRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.MimeType))
                return BadRequest("UserId and MimeType are required.");

            var (recordingId, uploadUri, blobUrl, blobPath) = await _recordingService.BeginUploadAsync(request.UserId, request.MimeType);
            return Ok(new BeginUploadResponse(recordingId, uploadUri.ToString(), blobUrl, blobPath, request.MimeType));
        }

        public record FinalizeRequest(int DurationSec, long SizeBytes, string? Notes);

        [HttpPost("{id:guid}/finalize")]
        public async Task<IActionResult> FinalizeRecording([FromRoute] Guid id, [FromBody] FinalizeRequest request)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (request.DurationSec < 0 || request.SizeBytes < 0) return BadRequest("DurationSec and SizeBytes must be non-negative.");

            await _recordingService.FinalizeAsync(id, request.DurationSec, request.SizeBytes, request.Notes);
            return NoContent();
        }

        [HttpGet("{id:guid}/read-sas")]
        public async Task<ActionResult<string>> GetReadSas([FromRoute] Guid id, [FromQuery] int? minutes)
        {
            var ttl = minutes.HasValue && minutes.Value > 0 ? TimeSpan.FromMinutes(minutes.Value) : TimeSpan.Zero;
            var uri = await _recordingService.GetReadSasAsync(id, ttl);
            return Ok(uri.ToString());
        }
    }
}
