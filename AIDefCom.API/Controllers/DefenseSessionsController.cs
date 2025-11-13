using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Services.DefenseSessionService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
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
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Defense sessions"),
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
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            return Ok(new ApiResponse<DefenseSessionReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Defense session"),
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
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Defense sessions by group"),
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
                throw new KeyNotFoundException($"No users found for defense session {id}");
            }

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Users for defense session"),
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
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
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
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            _logger.LogInformation("Defense session {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Defense session")
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
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            _logger.LogInformation("Defense session {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
