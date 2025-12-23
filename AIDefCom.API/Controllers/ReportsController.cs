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

        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetByLecturerId(string lecturerId)
        {
            _logger.LogInformation("Retrieving reports for lecturer ID: {LecturerId}", lecturerId);
            var data = await _service.GetByLecturerIdAsync(lecturerId);
            return Ok(new ApiResponse<IEnumerable<ReportReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Reports for lecturer"),
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
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.Deleted, "Report")
            });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            _logger.LogInformation("Approving report with ID: {Id}", id);
            var success = await _service.ApproveAsync(id);
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found for approval", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Approved, "Report")
            });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            _logger.LogInformation("Rejecting report with ID: {Id}", id);
            var success = await _service.RejectAsync(id);
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found for rejection", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Rejected, "Report")
            });
        }

        /// <summary>
        /// Lưu đường link PDF vào report (Admin and Lecturer)
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <param name="request">Object chứa file path</param>
        [HttpPut("{id}/filepath")]
        public async Task<IActionResult> SaveFilePath(int id, [FromBody] SaveFilePathRequest request)
        {
            _logger.LogInformation("Saving file path for report ID: {Id}", id);
            
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = "File path cannot be empty"
                });
            }

            var success = await _service.SaveReportFilePathAsync(id, request.FilePath);
            if (!success)
            {
                _logger.LogWarning("Report with ID {Id} not found", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }

            _logger.LogInformation("File path saved successfully for report ID: {Id}", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "File path saved successfully"
            });
        }

        /// <summary>
        /// Lấy đường link PDF của report kèm thông tin Defense Session
        /// </summary>
        /// <param name="id">Report ID</param>
        [HttpGet("{id}/filepath")]
        public async Task<IActionResult> GetFilePath(int id)
        {
            _logger.LogInformation("Retrieving file path with session info for report ID: {Id}", id);
            
            var filePathData = await _service.GetReportFilePathWithSessionAsync(id);
            if (filePathData == null)
            {
                _logger.LogWarning("Report with ID {Id} not found or has no file path", id);
                throw new KeyNotFoundException($"Report with ID {id} not found or has no file path");
            }

            return Ok(new ApiResponse<ReportFilePathDto>
            {
                Code = ResponseCodes.Success,
                Message = "File path retrieved successfully with session information",
                Data = filePathData
            });
        }
    }

    /// <summary>
    /// Request model để lưu file path
    /// </summary>
    public class SaveFilePathRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
