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
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all committee assignments");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<CommitteeAssignmentReadDto>>
            {
                MessageCode = MessageCodes.CommitteeAssignment_Success0001,
                Message = SystemMessages.CommitteeAssignment_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get committee assignment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving committee assignment with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.CommitteeAssignment_Fail0001,
                    Message = SystemMessages.CommitteeAssignment_Fail0001
                });
            }

            return Ok(new ApiResponse<CommitteeAssignmentReadDto>
            {
                MessageCode = MessageCodes.CommitteeAssignment_Success0002,
                Message = SystemMessages.CommitteeAssignment_Success0002,
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
                MessageCode = MessageCodes.CommitteeAssignment_Success0006,
                Message = SystemMessages.CommitteeAssignment_Success0006,
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
                MessageCode = MessageCodes.CommitteeAssignment_Success0007,
                Message = SystemMessages.CommitteeAssignment_Success0007,
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
                MessageCode = MessageCodes.CommitteeAssignment_Success0008,
                Message = SystemMessages.CommitteeAssignment_Success0008,
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
                MessageCode = MessageCodes.CommitteeAssignment_Success0003,
                Message = SystemMessages.CommitteeAssignment_Success0003,
                Data = new { id }
            });
        }

        /// <summary>
        /// Update an existing committee assignment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommitteeAssignmentUpdateDto dto)
        {
            _logger.LogInformation("Updating committee assignment with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.CommitteeAssignment_Fail0001,
                    Message = SystemMessages.CommitteeAssignment_Fail0001
                });
            }

            _logger.LogInformation("Committee assignment {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.CommitteeAssignment_Success0004,
                Message = SystemMessages.CommitteeAssignment_Success0004
            });
        }

        /// <summary>
        /// Delete a committee assignment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting committee assignment with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Committee assignment with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.CommitteeAssignment_Fail0001,
                    Message = SystemMessages.CommitteeAssignment_Fail0001
                });
            }

            _logger.LogInformation("Committee assignment {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
