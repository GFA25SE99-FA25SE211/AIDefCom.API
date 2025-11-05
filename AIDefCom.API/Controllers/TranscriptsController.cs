using AIDefCom.Service.Dto.Transcript;
using AIDefCom.Service.Services.TranscriptService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranscriptsController(ITranscriptService transcriptService, ILogger<TranscriptsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var transcripts = await transcriptService.GetAllAsync();
                return Ok(new
                {
                    message = "Transcripts retrieved successfully.",
                    data = transcripts
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all transcripts");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving transcripts.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var transcript = await transcriptService.GetByIdAsync(id);
                if (transcript == null)
                {
                    return NotFound(new { message = "Transcript not found." });
                }

                return Ok(new
                {
                    message = "Transcript retrieved successfully.",
                    data = transcript
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching transcript with ID {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the transcript.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            try
            {
                var transcripts = await transcriptService.GetBySessionIdAsync(sessionId);
                return Ok(new
                {
                    message = "Transcripts retrieved successfully.",
                    data = transcripts
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching transcripts for session {SessionId}", sessionId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving transcripts.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TranscriptCreateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input.",
                    errors = ModelState
                });
            }

            try
            {
                var id = await transcriptService.AddAsync(request);
                var createdTranscript = await transcriptService.GetByIdAsync(id);

                return CreatedAtAction(nameof(GetById), new { id }, new
                {
                    message = "Transcript created successfully.",
                    data = createdTranscript
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding transcript");
                return StatusCode(500, new
                {
                    message = "An error occurred while adding the transcript.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TranscriptUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input.",
                    errors = ModelState
                });
            }

            try
            {
                var success = await transcriptService.UpdateAsync(id, request);
                if (!success)
                {
                    return NotFound(new { message = "Transcript not found." });
                }

                return Ok(new { message = "Transcript updated successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating transcript with ID {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the transcript.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await transcriptService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Transcript not found." });
                }

                return Ok(new { message = "Transcript deleted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting transcript with ID {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the transcript.",
                    error = ex.Message
                });
            }
        }
    }
}
