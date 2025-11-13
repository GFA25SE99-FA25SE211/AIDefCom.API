using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Report;
using AIDefCom.Service.Services.ReportService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService service, ILogger<ReportsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all reports");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<ReportReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Reports"),
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving report with ID: {Id}", id);
            var report = await _service.GetByIdAsync(id);
            
            if (report == null)
            {
                _logger.LogWarning("Report with ID {Id} not found", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }

            return Ok(new ApiResponse<ReportReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Report"),
                Data = report
            });
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySessionId(int sessionId)
        {
            _logger.LogInformation("Retrieving reports for session ID: {SessionId}", sessionId);
            var data = await _service.GetBySessionIdAsync(sessionId);
            
            return Ok(new ApiResponse<IEnumerable<ReportReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Reports for session"),
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ReportCreateDto dto)
        {
            _logger.LogInformation("Creating new report for session {SessionId}", dto.SessionId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Report created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<ReportReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReportUpdateDto dto)
        {
            _logger.LogInformation("Updating report with ID: {Id}", id);
            var success = await _service.UpdateAsync(id, dto);
            
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }

            _logger.LogInformation("Report {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Report")
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting report with ID: {Id}", id);
            var success = await _service.DeleteAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }

            _logger.LogInformation("Report {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
