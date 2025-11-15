using System;
using System.Threading.Tasks;
using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Services.RecordingService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing recordings
    /// </summary>
    [ApiController]
    [Route("api/recordings")]
    public class RecordingsController : ControllerBase
    {
        private readonly IRecordingService _recordingService;
        private readonly ILogger<RecordingsController> _logger;

        public RecordingsController(IRecordingService recordingService, ILogger<RecordingsController> logger)
        {
            _recordingService = recordingService;
            _logger = logger;
        }

        public record BeginUploadRequest(string UserId, string MimeType);
        public record BeginUploadResponse(Guid RecordingId, string UploadUrl, string BlobUrl, string BlobPath, string MimeType);

        /// <summary>
        /// Begin upload process for a new recording
        /// </summary>
        [HttpPost("begin-upload")]
        public async Task<ActionResult<ApiResponse<BeginUploadResponse>>> BeginUpload([FromBody] BeginUploadRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.MimeType))
            {
                throw new ArgumentNullException(nameof(request), "UserId and MimeType are required");
            }

            _logger.LogInformation("Beginning upload for user {UserId} with MIME type {MimeType}", request.UserId, request.MimeType);
            var (recordingId, uploadUri, blobUrl, blobPath) = await _recordingService.BeginUploadAsync(request.UserId, request.MimeType);
            _logger.LogInformation("Upload initiated with Recording ID: {RecordingId}", recordingId);
            
            return Ok(new ApiResponse<BeginUploadResponse>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = new BeginUploadResponse(recordingId, uploadUri.ToString(), blobUrl, blobPath, request.MimeType)
            });
        }

        public record FinalizeRequest(int DurationSec, long SizeBytes, string? Notes);

        /// <summary>
        /// Finalize a recording after upload
        /// </summary>
        [HttpPost("{id:guid}/finalize")]
        public async Task<IActionResult> FinalizeRecording([FromRoute] Guid id, [FromBody] FinalizeRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "Request body is required");
            }

            if (request.DurationSec < 0 || request.SizeBytes < 0)
            {
                throw new ArgumentException("DurationSec and SizeBytes must be non-negative");
            }

            _logger.LogInformation("Finalizing recording {RecordingId} with duration {Duration}s and size {Size} bytes", id, request.DurationSec, request.SizeBytes);
            await _recordingService.FinalizeAsync(id, request.DurationSec, request.SizeBytes, request.Notes);
            _logger.LogInformation("Recording {RecordingId} finalized successfully", id);
            
            return NoContent();
        }

        /// <summary>
        /// Get read SAS URL for a recording
        /// </summary>
        [HttpGet("{id:guid}/read-sas")]
        public async Task<ActionResult<ApiResponse<string>>> GetReadSas([FromRoute] Guid id, [FromQuery] int? minutes)
        {
            _logger.LogInformation("Generating read SAS URL for recording {RecordingId} with TTL {Minutes} minutes", id, minutes);
            var ttl = minutes.HasValue && minutes.Value > 0 ? TimeSpan.FromMinutes(minutes.Value) : TimeSpan.Zero;
            var uri = await _recordingService.GetReadSasAsync(id, ttl);
            
            return Ok(new ApiResponse<string>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = uri.ToString()
            });
        }
    }
}
