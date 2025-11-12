using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Transcript;
using AIDefCom.Service.Services.TranscriptService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing transcripts
    /// </summary>
    [Route("api/transcripts")]
    [ApiController]
    public class TranscriptsController : ControllerBase
    {
        private readonly ITranscriptService _transcriptService;
        private readonly ILogger<TranscriptsController> _logger;

        public TranscriptsController(ITranscriptService transcriptService, ILogger<TranscriptsController> logger)
        {
            _transcriptService = transcriptService;
            _logger = logger;
        }

        /// <summary>
        /// Get all transcripts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all transcripts");
            var transcripts = await _transcriptService.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<TranscriptReadDto>>
            {
                MessageCode = MessageCodes.Transcript_Success0001,
                Message = SystemMessages.Transcript_Success0001,
                Data = transcripts
            });
        }

        /// <summary>
        /// Get transcript by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving transcript with ID: {Id}", id);
            var transcript = await _transcriptService.GetByIdAsync(id);
            
            if (transcript == null)
            {
                _logger.LogWarning("Transcript with ID {Id} not found", id);
                throw new KeyNotFoundException($"Transcript with ID {id} not found");
            }

            return Ok(new ApiResponse<TranscriptReadDto>
            {
                MessageCode = MessageCodes.Transcript_Success0002,
                Message = SystemMessages.Transcript_Success0002,
                Data = transcript
            });
        }

        /// <summary>
        /// Get transcripts by session ID
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            _logger.LogInformation("Retrieving transcripts for session ID: {SessionId}", sessionId);
            var transcripts = await _transcriptService.GetBySessionIdAsync(sessionId);
            
            return Ok(new ApiResponse<IEnumerable<TranscriptReadDto>>
            {
                MessageCode = MessageCodes.Transcript_Success0001,
                Message = SystemMessages.Transcript_Success0001,
                Data = transcripts
            });
        }

        /// <summary>
        /// Create a new transcript
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TranscriptCreateDto request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid transcript data");
            }

            _logger.LogInformation("Creating new transcript for session {SessionId}", request.SessionId);
            var id = await _transcriptService.AddAsync(request);
            var createdTranscript = await _transcriptService.GetByIdAsync(id);
            _logger.LogInformation("Transcript created with ID: {Id}", id);

            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<TranscriptReadDto>
            {
                MessageCode = MessageCodes.Transcript_Success0003,
                Message = SystemMessages.Transcript_Success0003,
                Data = createdTranscript
            });
        }

        /// <summary>
        /// Update an existing transcript
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TranscriptUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid transcript data");
            }

            _logger.LogInformation("Updating transcript with ID: {Id}", id);
            var success = await _transcriptService.UpdateAsync(id, request);
            
            if (!success)
            {
                _logger.LogWarning("Transcript with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Transcript with ID {id} not found");
            }

            _logger.LogInformation("Transcript {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Transcript_Success0004,
                Message = SystemMessages.Transcript_Success0004
            });
        }

        /// <summary>
        /// Delete a transcript
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting transcript with ID: {Id}", id);
            var success = await _transcriptService.DeleteAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Transcript with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Transcript with ID {id} not found");
            }

            _logger.LogInformation("Transcript {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
