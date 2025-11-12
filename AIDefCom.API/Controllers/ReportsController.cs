using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Report;
using AIDefCom.Service.Services.ReportService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing reports
    /// </summary>
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

        /// <summary>
        /// Get all reports
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all reports");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<ReportReadDto>>
            {
                MessageCode = MessageCodes.Report_Success0001,
                Message = SystemMessages.Report_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get report by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving report with ID: {Id}", id);
            var report = await _service.GetByIdAsync(id);
            if (report == null)
            {
                _logger.LogWarning("Report with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Report_Fail0001,
                    Message = SystemMessages.Report_Fail0001
                });
            }

            return Ok(new ApiResponse<ReportReadDto>
            {
                MessageCode = MessageCodes.Report_Success0002,
                Message = SystemMessages.Report_Success0002,
                Data = report
            });
        }

        /// <summary>
        /// Get reports by session ID
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySessionId(int sessionId)
        {
            _logger.LogInformation("Retrieving reports for session ID: {SessionId}", sessionId);
            var data = await _service.GetBySessionIdAsync(sessionId);
            return Ok(new ApiResponse<IEnumerable<ReportReadDto>>
            {
                MessageCode = MessageCodes.Report_Success0006,
                Message = SystemMessages.Report_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Create a new report
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ReportCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Creating new report for session {SessionId}", dto.SessionId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Report created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<ReportReadDto>
            {
                MessageCode = MessageCodes.Report_Success0003,
                Message = SystemMessages.Report_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing report
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReportUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Updating report with ID: {Id}", id);
            var success = await _service.UpdateAsync(id, dto);
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Report_Fail0001,
                    Message = SystemMessages.Report_Fail0001
                });
            }

            _logger.LogInformation("Report {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Report_Success0004,
                Message = SystemMessages.Report_Success0004
            });
        }

        /// <summary>
        /// Delete a report
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting report with ID: {Id}", id);
            var success = await _service.DeleteAsync(id);
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Report_Fail0001,
                    Message = SystemMessages.Report_Fail0001
                });
            }

            _logger.LogInformation("Report {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
