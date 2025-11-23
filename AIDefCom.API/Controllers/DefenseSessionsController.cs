using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Dto.Import;
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
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all defense sessions (includeDeleted: {IncludeDeleted})", includeDeleted);
            var data = await _service.GetAllAsync(includeDeleted);
            
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
        /// Get defense sessions by lecturer ID
        /// </summary>
        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetByLecturerId(string lecturerId)
        {
            _logger.LogInformation("Retrieving defense sessions for lecturer ID: {LecturerId}", lecturerId);
            var data = await _service.GetByLecturerIdAsync(lecturerId);
            
            return Ok(new ApiResponse<IEnumerable<DefenseSessionReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Defense sessions by lecturer"),
                Data = data
            });
        }

        /// <summary>
        /// Get lecturer's role in a specific defense session
        /// </summary>
        [HttpGet("{defenseSessionId}/lecturer/{lecturerId}/role")]
        public async Task<IActionResult> GetLecturerRoleInDefenseSession(string lecturerId, int defenseSessionId)
        {
            _logger.LogInformation("Retrieving role for lecturer {LecturerId} in defense session {DefenseSessionId}", 
                lecturerId, defenseSessionId);
            
            var roleName = await _service.GetLecturerRoleInDefenseSessionAsync(lecturerId, defenseSessionId);
            
            if (roleName == null)
            {
                _logger.LogWarning("Lecturer {LecturerId} not found in defense session {DefenseSessionId}", 
                    lecturerId, defenseSessionId);
                return NotFound(new ApiResponse<object>
                {
                    Code = ResponseCodes.NotFound,
                    Message = "Lecturer not found in this defense session or defense session does not exist",
                    Data = null
                });
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Lecturer role retrieved successfully",
                Data = new { LecturerId = lecturerId, DefenseSessionId = defenseSessionId, RoleName = roleName }
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
        /// Soft delete a defense session
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft deleting defense session with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Defense session with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            _logger.LogInformation("Defense session {Id} soft deleted successfully", id);
            return NoContent();
        }

        /// <summary>
        /// Restore a soft-deleted defense session
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring defense session with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Defense session with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            _logger.LogInformation("Defense session {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Defense session restored successfully"
            });
        }

        /// <summary>
        /// Import defense sessions from Excel file
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportDefenseSessions(IFormFile file)
        {
            _logger.LogInformation("Starting defense session import from Excel");

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = "File is required",
                    Data = null
                });
            }

            try
            {
                var result = await _service.ImportDefenseSessionsAsync(file);

                _logger.LogInformation(
                    "Defense session import completed. Success: {Success}, Failures: {Failures}",
                    result.SuccessCount, result.FailureCount);

                return Ok(new ApiResponse<DefenseSessionImportResultDto>
                {
                    Code = ResponseCodes.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during defense session import");
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Download Excel template for defense session import
        /// </summary>
        [HttpGet("import/template")]
        public IActionResult DownloadDefenseSessionTemplate()
        {
            _logger.LogInformation("Generating defense session import template");

            var fileBytes = _service.GenerateDefenseSessionTemplate();
            var fileName = $"DefenseSession_Import_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
