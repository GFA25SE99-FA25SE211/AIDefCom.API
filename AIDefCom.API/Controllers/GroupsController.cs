using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Group;
using AIDefCom.Service.Services.GroupService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
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

            _logger.LogInformation("Group {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Group")
            });
        }

        /// <summary>
        /// Update total score for a group
        /// </summary>
        [HttpPut("{id}/total-score")]
        public async Task<IActionResult> UpdateTotalScore(string id, [FromBody] GroupTotalScoreUpdateDto dto)
        {
            _logger.LogInformation("Updating total score for group with ID: {Id}, Score: {Score}", id, dto.TotalScore);
            var ok = await _service.UpdateTotalScoreAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for total score update", id);
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }

            _logger.LogInformation("Group {Id} total score updated successfully to {Score}", id, dto.TotalScore);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Group total score updated successfully"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Soft deleting group with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Group with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Group with ID {id} not found");
            }

            _logger.LogInformation("Group {Id} soft deleted successfully", id);
            return NoContent();
        }

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

            _logger.LogInformation("Group {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Group restored successfully"
            });
        }
    }
}
