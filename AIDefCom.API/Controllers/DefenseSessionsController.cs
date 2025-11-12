using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Services.DefenseSessionService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing defense sessions
    /// </summary>
    [Route("api/defense-sessions")]
    [ApiController]
    public class DefenseSessionsController : ControllerBase
    {
        private readonly IDefenseSessionService _service;
        private readonly ILogger<DefenseSessionsController> _logger;

        public DefenseSessionsController(IDefenseSessionService service, ILogger<DefenseSessionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all defense sessions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all defense sessions");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<DefenseSessionReadDto>>
            {
                MessageCode = MessageCodes.DefenseSession_Success0001,
                Message = SystemMessages.DefenseSession_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get defense session by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving defense session with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Defense session with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.DefenseSession_Fail0001,
                    Message = SystemMessages.DefenseSession_Fail0001
                });
            }

            return Ok(new ApiResponse<DefenseSessionReadDto>
            {
                MessageCode = MessageCodes.DefenseSession_Success0002,
                Message = SystemMessages.DefenseSession_Success0002,
                Data = item
            });
        }

        /// <summary>
        /// Get defense sessions by group ID
        /// </summary>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            _logger.LogInformation("Retrieving defense sessions for group ID: {GroupId}", groupId);
            var data = await _service.GetByGroupIdAsync(groupId);
            return Ok(new ApiResponse<IEnumerable<DefenseSessionReadDto>>
            {
                MessageCode = MessageCodes.DefenseSession_Success0006,
                Message = SystemMessages.DefenseSession_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Get users by defense session ID
        /// </summary>
        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsersByDefenseSessionId(int id)
        {
            _logger.LogInformation("Retrieving users for defense session ID: {Id}", id);
            var users = await _service.GetUsersByDefenseSessionIdAsync(id);
            if (!users.Any())
            {
                _logger.LogWarning("No users found for defense session ID {Id}", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.DefenseSession_Fail0002,
                    Message = SystemMessages.DefenseSession_Fail0002
                });
            }

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                MessageCode = MessageCodes.DefenseSession_Success0007,
                Message = SystemMessages.DefenseSession_Success0007,
                Data = users
            });
        }

        /// <summary>
        /// Create a new defense session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DefenseSessionCreateDto dto)
        {
            _logger.LogInformation("Creating new defense session for group {GroupId}", dto.GroupId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Defense session created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<DefenseSessionReadDto>
            {
                MessageCode = MessageCodes.DefenseSession_Success0003,
                Message = SystemMessages.DefenseSession_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing defense session
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DefenseSessionUpdateDto dto)
        {
            _logger.LogInformation("Updating defense session with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Defense session with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.DefenseSession_Fail0001,
                    Message = SystemMessages.DefenseSession_Fail0001
                });
            }

            _logger.LogInformation("Defense session {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.DefenseSession_Success0004,
                Message = SystemMessages.DefenseSession_Success0004
            });
        }

        /// <summary>
        /// Delete a defense session
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting defense session with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Defense session with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.DefenseSession_Fail0001,
                    Message = SystemMessages.DefenseSession_Fail0001
                });
            }

            _logger.LogInformation("Defense session {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
