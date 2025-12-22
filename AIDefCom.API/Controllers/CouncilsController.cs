using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Council;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Services.CouncilService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing councils
    /// </summary>
    [Route("api/councils")]
    [ApiController]
    public class CouncilsController : ControllerBase
    {
        private readonly ICouncilService _service;
        private readonly ILogger<CouncilsController> _logger;

        public CouncilsController(ICouncilService service, ILogger<CouncilsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all councils
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            _logger.LogInformation("Retrieving all councils (includeInactive: {IncludeInactive})", includeInactive);
            var data = await _service.GetAllAsync(includeInactive);
            
            return Ok(new ApiResponse<IEnumerable<CouncilReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Councils"),
                Data = data
            });
        }

        /// <summary>
        /// Get council by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving council with ID: {Id}", id);
            var entity = await _service.GetByIdAsync(id);
            
            if (entity == null)
            {
                _logger.LogWarning("Council with ID {Id} not found", id);
                throw new KeyNotFoundException($"Council with ID {id} not found");
            }

            return Ok(new ApiResponse<CouncilReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Council"),
                Data = entity
            });
        }

        /// <summary>
        /// Create a new council (Admin and Moderator)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CouncilCreateDto dto)
        {
            _logger.LogInformation("Creating new council");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Council created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<CouncilReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing council (Admin and Moderator)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CouncilUpdateDto dto)
        {
            _logger.LogInformation("Updating council with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Council with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Council with ID {id} not found");
            }

            _logger.LogInformation("Council {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Council")
            });
        }

        /// <summary>
        /// Soft delete a council (Admin and Moderator)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            _logger.LogInformation("Soft deleting council with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Council with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Council with ID {id} not found");
            }

            _logger.LogInformation("Council {Id} soft deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.SoftDeleted, "Council")
            });
        }

        /// <summary>
        /// Restore a soft-deleted council (Admin and Moderator)
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring council with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Council with ID {Id} not found or already active for restoration", id);
                throw new KeyNotFoundException($"Council with ID {id} not found or already active");
            }

            _logger.LogInformation("Council {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Council")
            });
        }

        /// <summary>
        /// Import councils with committee assignments from Excel file (Admin and Moderator)
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportCouncilsWithCommittees([FromForm] CouncilCommitteeImportRequestDto request)
        {
            _logger.LogInformation("Starting council & committee import. MajorId: {MajorId}", request.MajorId);

            if (request.File == null || request.File.Length == 0)
            {
                throw new ArgumentNullException(nameof(request.File), "File is required");
            }

            if (request.MajorId <= 0)
            {
                throw new ArgumentException("Valid MajorId is required");
            }

            var result = await _service.ImportCouncilsWithCommitteesAsync(request.MajorId, request.File);

            _logger.LogInformation(
                "Council & committee import completed. Success: {Success}, Failures: {Failures}, Councils: {Councils}, Assignments: {Assignments}",
                result.SuccessCount, result.FailureCount, result.CreatedCouncilIds.Count, result.CreatedCommitteeAssignmentIds.Count);

            // ✅ Return different HTTP status codes based on import results
            // HTTP 200: All success
            // HTTP 207: Partial success (some rows succeeded, some failed)
            // HTTP 400: All failed

            if (result.FailureCount == 0)
            {
                // All rows succeeded
                return Ok(new ApiResponse<CouncilCommitteeImportResultDto>
                {
                    Code = ResponseCodes.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            else if (result.SuccessCount > 0)
            {
                // Partial success - return HTTP 207 Multi-Status
                return StatusCode(207, new ApiResponse<CouncilCommitteeImportResultDto>
                {
                    Code = ResponseCodes.MultiStatus,
                    Message = result.Message,
                    Data = result
                });
            }
            else
            {
                // All failed - return HTTP 400 Bad Request
                return BadRequest(new ApiResponse<CouncilCommitteeImportResultDto>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = result.Message,
                    Data = result
                });
            }
        }

        /// <summary>
        /// Download Excel template for council & committee import (Admin và Moderator)
        /// </summary>
        [HttpGet("import/template")]
        public IActionResult DownloadCouncilCommitteeTemplate()
        {
            _logger.LogInformation("Generating council & committee import template");

            var fileBytes = _service.GenerateCouncilCommitteeTemplate();
            var fileName = $"Council_Committee_Import_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
