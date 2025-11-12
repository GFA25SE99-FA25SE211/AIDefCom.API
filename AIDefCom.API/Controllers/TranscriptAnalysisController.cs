using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.TranscriptAnalysis;
using AIDefCom.Service.Services.TranscriptAnalysisService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for analyzing transcripts using AI
    /// </summary>
    [Route("api/transcript-analysis")]
    [ApiController]
    public class TranscriptAnalysisController : ControllerBase
    {
        private readonly ITranscriptAnalysisService _analysisService;
        private readonly ILogger<TranscriptAnalysisController> _logger;

        public TranscriptAnalysisController(ITranscriptAnalysisService analysisService, ILogger<TranscriptAnalysisController> logger)
        {
            _analysisService = analysisService;
            _logger = logger;
        }

        /// <summary>
        /// Analyze a transcript using AI
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AnalyzeTranscript([FromBody] TranscriptAnalysisRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid transcript analysis request");
            }

            _logger.LogInformation("Starting transcript analysis for SessionId: {SessionId}", request.DefenseSessionId);
            var result = await _analysisService.AnalyzeTranscriptAsync(request);
            _logger.LogInformation("Transcript analysis completed for SessionId: {SessionId}", request.DefenseSessionId);

            return Ok(new ApiResponse<TranscriptAnalysisResponseDto>
            {
                MessageCode = MessageCodes.TranscriptAnalysis_Success0001,
                Message = SystemMessages.TranscriptAnalysis_Success0001,
                Data = result
            });
        }
    }
}
