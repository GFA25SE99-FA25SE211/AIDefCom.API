using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Group;
using AIDefCom.Service.Services.GroupService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize] // Tất cả endpoints yêu cầu authenticated
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _service;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupService service, ILogger<GroupsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all groups (includeDeleted: {IncludeDeleted})", includeDeleted);
            var data = await _service.GetAllAsync(includeDeleted);
            return Ok(new ApiResponse<IEnumerable<GroupReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Groups"),
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Retrieving group with ID: {Id}", id);
            var group = await _service.GetByIdAsync(id);
            if (group == null)
            {
                _logger.LogWarning("Group with ID {Id} not found", id);
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }
            return Ok(new ApiResponse<GroupReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Group"),
                Data = group
            });
        }

        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetBySemesterId(int semesterId)
        {
            _logger.LogInformation("Retrieving groups for semester ID: {SemesterId}", semesterId);
            var data = await _service.GetBySemesterIdAsync(semesterId);
            return Ok(new ApiResponse<IEnumerable<GroupReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Groups by semester"),
                Data = data
            });
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] GroupCreateDto dto)
        {
            _logger.LogInformation("Creating new group");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Group created with ID: {Id}", id);
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<GroupReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] GroupUpdateDto dto)
        {
            _logger.LogInformation("Updating group with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Group")
            });
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Deleting group with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.Deleted, "Group")
            });
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(string id)
        {
            _logger.LogInformation("Restoring group with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Group")
            });
        }
    }
}
