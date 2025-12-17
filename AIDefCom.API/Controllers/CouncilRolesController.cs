using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.CouncilRole;
using AIDefCom.Service.Services.CouncilRoleService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/council-roles")]
    [ApiController]
    public class CouncilRolesController : ControllerBase
    {
        private readonly ICouncilRoleService _service;
        private readonly ILogger<CouncilRolesController> _logger;

        public CouncilRolesController(ICouncilRoleService service, ILogger<CouncilRolesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all council roles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all council roles (includeDeleted: {IncludeDeleted})", includeDeleted);
            var data = await _service.GetAllAsync(includeDeleted);

            return Ok(new ApiResponse<IEnumerable<CouncilRoleReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Council roles"),
                Data = data
            });
        }

        /// <summary>
        /// Get council role by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving council role with ID: {Id}", id);
            var role = await _service.GetByIdAsync(id);

            if (role == null)
            {
                _logger.LogWarning("Council role with ID {Id} not found", id);
                throw new KeyNotFoundException($"Council role with ID {id} not found");
            }

            return Ok(new ApiResponse<CouncilRoleReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Council role"),
                Data = role
            });
        }

        /// <summary>
        /// Get council role by role name
        /// </summary>
        [HttpGet("by-name/{roleName}")]
        public async Task<IActionResult> GetByRoleName(string roleName)
        {
            _logger.LogInformation("Retrieving council role with name: {RoleName}", roleName);
            var role = await _service.GetByRoleNameAsync(roleName);

            if (role == null)
            {
                _logger.LogWarning("Council role with name '{RoleName}' not found", roleName);
                throw new KeyNotFoundException($"Council role with name '{roleName}' not found");
            }

            return Ok(new ApiResponse<CouncilRoleReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Council role"),
                Data = role
            });
        }

        /// <summary>
        /// Create a new council role
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CouncilRoleCreateDto dto)
        {
            _logger.LogInformation("Creating new council role: {RoleName}", dto.RoleName);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Council role created with ID: {Id}", id);

            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<CouncilRoleReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing council role
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CouncilRoleUpdateDto dto)
        {
            _logger.LogInformation("Updating council role with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);

            if (!ok)
            {
                _logger.LogWarning("Council role with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Council role with ID {id} not found");
            }

            _logger.LogInformation("Council role {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Council role")
            });
        }

        /// <summary>
        /// Soft delete a council role
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft deleting council role with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);

            if (!ok)
            {
                _logger.LogWarning("Council role with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Council role with ID {id} not found");
            }

            _logger.LogInformation("Council role {Id} soft deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Deleted, "Council role")
            });
        }

        /// <summary>
        /// Restore a soft-deleted council role
        /// </summary>
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring council role with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);

            if (!ok)
            {
                _logger.LogWarning("Council role with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Council role with ID {id} not found");
            }

            _logger.LogInformation("Council role {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Council role restored successfully"
            });
        }
    }
}
