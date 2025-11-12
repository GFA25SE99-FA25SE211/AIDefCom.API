using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Group;
using AIDefCom.Service.Services.GroupService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing groups
    /// </summary>
    [Route("api/groups")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _service;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupService service, ILogger<GroupsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all groups
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all groups");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<GroupReadDto>>
            {
                MessageCode = MessageCodes.Group_Success0001,
                Message = SystemMessages.Group_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get group by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Retrieving group with ID: {Id}", id);
            var group = await _service.GetByIdAsync(id);
            if (group == null)
            {
                _logger.LogWarning("Group with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Group_Fail0001,
                    Message = SystemMessages.Group_Fail0001
                });
            }

            return Ok(new ApiResponse<GroupReadDto>
            {
                MessageCode = MessageCodes.Group_Success0002,
                Message = SystemMessages.Group_Success0002,
                Data = group
            });
        }

        /// <summary>
        /// Get groups by semester ID
        /// </summary>
        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetBySemesterId(int semesterId)
        {
            _logger.LogInformation("Retrieving groups for semester ID: {SemesterId}", semesterId);
            var data = await _service.GetBySemesterIdAsync(semesterId);
            return Ok(new ApiResponse<IEnumerable<GroupReadDto>>
            {
                MessageCode = MessageCodes.Group_Success0006,
                Message = SystemMessages.Group_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Create a new group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] GroupCreateDto dto)
        {
            _logger.LogInformation("Creating new group");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Group created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<GroupReadDto>
            {
                MessageCode = MessageCodes.Group_Success0003,
                Message = SystemMessages.Group_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing group
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] GroupUpdateDto dto)
        {
            _logger.LogInformation("Updating group with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Group_Fail0001,
                    Message = SystemMessages.Group_Fail0001
                });
            }

            _logger.LogInformation("Group {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Group_Success0004,
                Message = SystemMessages.Group_Success0004
            });
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Deleting group with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Group_Fail0001,
                    Message = SystemMessages.Group_Fail0001
                });
            }

            _logger.LogInformation("Group {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
