using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.DefenseReport;
using AIDefCom.Service.Services.DefenseReportService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for generating defense reports (Biên b?n b?o v?)
    /// </summary>
    [Route("api/defense-reports")]
    [ApiController]
    public class DefenseReportsController : ControllerBase
    {
        private readonly IDefenseReportService _reportService;
        private readonly ILogger<DefenseReportsController> _logger;

        public DefenseReportsController(
            IDefenseReportService reportService,
            ILogger<DefenseReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Generate defense report from transcript ID
        /// </summary>
        /// <param name="request">Request containing transcript ID</param>
        /// <returns>Defense report with council info, session info, project info, and AI-analyzed defense progress</returns>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateDefenseReport([FromBody] DefenseReportRequestDto request)
        {
            _logger.LogInformation("?? Generating defense report for transcript ID: {TranscriptId}", request.TranscriptId);
            
            var result = await _reportService.GenerateDefenseReportAsync(request);
            
            _logger.LogInformation("? Defense report generated successfully for transcript ID: {TranscriptId}", request.TranscriptId);

            return Ok(new ApiResponse<DefenseReportResponseDto>
            {
                Code = ResponseCodes.Success,
                Message = "Defense report generated successfully",
                Data = result
            });
        }
    }
}
