using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.CommitteeAssignment;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Services.CommitteeAssignmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing committee assignments
    /// </summary>
    [Route("api/committee-assignments")]
    [ApiController]
    [Authorize(Roles = "Admin,Moderator")] // Default: Admin và Moderator có quyền truy cập
    public class CommitteeAssignmentsController : ControllerBase
    {
        private readonly ICommitteeAssignmentService _service;
        private readonly ILogger<CommitteeAssignmentsController> _logger;

        public CommitteeAssignmentsController(ICommitteeAssignmentService service, ILogger<CommitteeAssignmentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all committee assignments (All authenticated users)
        /// </summary>
        [HttpGet]
        [Authorize] // Override: Tất cả user đã authenticated đều được xem
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all committee assignments (includeDeleted: {IncludeDeleted})", includeDeleted);
            var data = await _service.GetAllAsync(includeDeleted);
            return Ok(new ApiResponse<IEnumerable<CommitteeAssignmentReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Committee assignments"),
                Data = data
            });
        }

        /// <summary>
        /// Get committee assignment by ID (Admin and Moderator)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Retrieving committee assignment with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found", id);
                throw new KeyNotFoundException($"Committee assignment with ID {id} not found");
            }
            return Ok(new ApiResponse<CommitteeAssignmentReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Committee assignment"),
                Data = item
            });
        }

        /// <summary>
        /// Get committee assignments by council ID (All authenticated users)
        /// </summary>
        [HttpGet("council/{councilId}")]
        [Authorize] // Override: Tất cả user đã authenticated đều được xem
        public async Task<IActionResult> GetByCouncilId(int councilId)
        {
            _logger.LogInformation("Retrieving committee assignments for council ID: {CouncilId}", councilId);
            var data = await _service.GetByCouncilIdAsync(councilId);
            return Ok(new ApiResponse<IEnumerable<CommitteeAssignmentReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Committee assignments by council"),
                Data = data
            });
        }

        /// <summary>
        /// Get committee assignments by session ID (Admin and Moderator)
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySessionId(int sessionId)
        {
            _logger.LogInformation("Retrieving committee assignments for session ID: {SessionId}", sessionId);
            var data = await _service.GetBySessionIdAsync(sessionId);
            return Ok(new ApiResponse<IEnumerable<CommitteeAssignmentReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Committee assignments by session"),
                Data = data
            });
        }

        /// <summary>
        /// Get committee assignments by lecturer ID (Admin and Moderator)
        /// </summary>
        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetByLecturerId(string lecturerId)
        {
            _logger.LogInformation("Retrieving committee assignments for lecturer ID: {LecturerId}", lecturerId);
            var data = await _service.GetByLecturerIdAsync(lecturerId);
            return Ok(new ApiResponse<IEnumerable<CommitteeAssignmentReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Committee assignments by lecturer"),
                Data = data
            });
        }

        /// <summary>
        /// Get CommitteeAssignment ID by Lecturer ID and Defense Session ID (Admin and Moderator)
        /// </summary>
        /// <param name="lecturerId">The Lecturer ID</param>
        /// <param name="sessionId">The Defense Session ID</param>
        /// <returns>CommitteeAssignment ID or null if not found</returns>
        [HttpGet("lecturer/{lecturerId}/session/{sessionId}/id")]
        public async Task<IActionResult> GetIdByLecturerIdAndSessionId(string lecturerId, int sessionId)
        {
            _logger.LogInformation("Retrieving committee assignment ID for lecturer {LecturerId} in session {SessionId}", lecturerId, sessionId);
            var id = await _service.GetIdByLecturerIdAndSessionIdAsync(lecturerId, sessionId);
            
            if (id == null)
            {
                _logger.LogWarning("Committee assignment not found for lecturer {LecturerId} in session {SessionId}", lecturerId, sessionId);
                return NotFound(new ApiResponse<object>
                {
                    Code = ResponseCodes.NotFound,
                    Message = $"No committee assignment found for lecturer '{lecturerId}' in session {sessionId}",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                Code = ResponseCodes.Success,
                Message = "Committee assignment ID retrieved successfully",
                Data = id
            });
        }

        /// <summary>
        /// Create a new committee assignment (Admin and Moderator)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CommitteeAssignmentCreateDto dto)
        {
            _logger.LogInformation("Creating new committee assignment for lecturer {LecturerId}", dto.LecturerId);
            var id = await _service.AddAsync(dto);
            _logger.LogInformation("Committee assignment created with ID: {Id}", id);
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = new { id }
            });
        }

        /// <summary>
        /// Update an existing committee assignment (Admin and Moderator)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CommitteeAssignmentUpdateDto dto)
        {
            _logger.LogInformation("Updating committee assignment with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Committee assignment with ID {id} not found");
            }
            _logger.LogInformation("Committee assignment {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Committee assignment")
            });
        }

        /// <summary>
        /// Soft delete a committee assignment (Admin and Moderator)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Soft deleting committee assignment with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Committee assignment with ID {id} not found");
            }
            _logger.LogInformation("Committee assignment {Id} soft deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.SoftDeleted, "Committee assignment")
            });
        }

        /// <summary>
        /// Restore a soft-deleted committee assignment (Admin and Moderator)
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(string id)
        {
            _logger.LogInformation("Restoring committee assignment with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Committee assignment with ID {id} not found");
            }
            _logger.LogInformation("Committee assignment {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Committee assignment")
            });
        }
    }
}
