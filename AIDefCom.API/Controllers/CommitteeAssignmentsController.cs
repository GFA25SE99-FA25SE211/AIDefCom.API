using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.CommitteeAssignment;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Services.CommitteeAssignmentService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing committee assignments
    /// </summary>
    [Route("api/committee-assignments")]
    [ApiController]
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
        /// Get all committee assignments
        /// </summary>
        [HttpGet]
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
        /// Get committee assignment by ID
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
        /// Get committee assignments by council ID
        /// </summary>
        [HttpGet("council/{councilId}")]
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
        /// Get committee assignments by session ID
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
        /// Get committee assignments by lecturer ID
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
        /// Create a new committee assignment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CommitteeAssignmentCreateDto dto)
        {
            _logger.LogInformation("Creating new committee assignment for lecturer {LecturerId}", dto.LecturerId);
            var id = await _service.AddAsync(dto);
            _logger.LogInformation("Committee assignment created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetAll), new { id }, new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = string.Format(ResponseMessages.Created),
                Data = new { id }
            });
        }

        /// <summary>
        /// Update an existing committee assignment
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
        /// Soft delete a committee assignment
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
            return NoContent();
        }

        /// <summary>
        /// Restore a soft-deleted committee assignment
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
                Message = "Committee assignment restored successfully"
            });
        }
    }
}
