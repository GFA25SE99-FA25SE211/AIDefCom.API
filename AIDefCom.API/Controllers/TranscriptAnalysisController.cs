using AIDefCom.Service.Dto.TranscriptAnalysis;
using AIDefCom.Service.Services.TranscriptAnalysisService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/transcript")]
    [ApiController]
    public class TranscriptAnalysisController(
        ITranscriptAnalysisService analysisService,
        ILogger<TranscriptAnalysisController> logger) : ControllerBase
    {
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeTranscript([FromBody] TranscriptAnalysisRequestDto request)
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
                logger.LogInformation("Starting transcript analysis for SessionId: {SessionId}", request.DefenseSessionId);

                var result = await analysisService.AnalyzeTranscriptAsync(request);

                return Ok(new
                {
                    message = "Transcript analyzed successfully.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Configuration error during transcript analysis");
                return StatusCode(500, new
                {
                    message = "AI service is not properly configured.",
                    error = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error calling AI API");
                return StatusCode(502, new
                {
                    message = "Failed to communicate with AI service.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during transcript analysis");
                return StatusCode(500, new
                {
                    message = "An error occurred while analyzing the transcript.",
                    error = ex.Message
                });
            }
        }
    }
}
