using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Services.DefenseSessionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/defense-sessions")]
    [ApiController]
    [Authorize] // Tất cả endpoints yêu cầu authenticated (bao gồm Student)
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
        /// Get defense sessions by student ID
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(string studentId)
        {
            _logger.LogInformation("Retrieving defense sessions for student ID: {StudentId}", studentId);
            var data = await _service.GetByStudentIdAsync(studentId);
            
            return Ok(new ApiResponse<IEnumerable<DefenseSessionReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Defense sessions by student"),
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
                throw new KeyNotFoundException("Lecturer not found in this defense session or defense session does not exist");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Lecturer role"),
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
        /// Create a new defense session (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DefenseSessionCreateDto dto)
        {
            _logger.LogInformation("Creating new defense session for group {GroupId}", dto.GroupId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<DefenseSessionReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing defense session (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DefenseSessionUpdateDto dto)
        {
            _logger.LogInformation("Updating defense session with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Defense session")
            });
        }

        /// <summary>
        /// Soft delete a defense session (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft deleting defense session with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            
            if (!ok)
            {
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.SoftDeleted, "Defense session")
            });
        }

        /// <summary>
        /// Restore a soft-deleted defense session (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring defense session with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            
            if (!ok)
            {
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Defense session")
            });
        }

        /// <summary>
        /// Change defense session status to InProgress (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}/start")]
        public async Task<IActionResult> StartSession(int id)
        {
            _logger.LogInformation("Starting defense session with ID: {Id}", id);
            var ok = await _service.ChangeStatusAsync(id, "InProgress");
            
            if (!ok)
            {
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            var updated = await _service.GetByIdAsync(id);
            return Ok(new ApiResponse<DefenseSessionReadDto>
            {
                Code = ResponseCodes.Success,
                Message = "Defense session started successfully",
                Data = updated
            });
        }

        /// <summary>
        /// Change defense session status to Completed (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteSession(int id)
        {
            _logger.LogInformation("Completing defense session with ID: {Id}", id);
            var ok = await _service.ChangeStatusAsync(id, "Completed");
            
            if (!ok)
            {
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            var updated = await _service.GetByIdAsync(id);
            return Ok(new ApiResponse<DefenseSessionReadDto>
            {
                Code = ResponseCodes.Success,
                Message = "Defense session completed successfully",
                Data = updated
            });
        }

        [Authorize(Roles = "Admin,Lecturer")]
        [HttpPut("{id}/total-score")]
        public async Task<IActionResult> UpdateTotalScore(int id, [FromBody] DefenseSessionTotalScoreUpdateDto dto)
        {
            _logger.LogInformation("Updating total score for defense session with ID: {Id}, Score: {Score}", id, dto.TotalScore);
            var ok = await _service.UpdateTotalScoreAsync(id, dto);

            if (!ok)
            {
                throw new KeyNotFoundException($"Defense session with ID {id} not found");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.TotalScoreUpdated, "Defense session")
            });
        }

        
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost("import")]
        public async Task<IActionResult> ImportDefenseSessions(IFormFile file)
        {
            _logger.LogInformation("Starting defense session import from Excel");

            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file), "File is required");
            }

            var result = await _service.ImportDefenseSessionsAsync(file);

            // ✅ Return different HTTP status codes based on import results
            // HTTP 200: All success
            // HTTP 207: Partial success (some rows succeeded, some failed)
            // HTTP 400: All failed

            if (result.FailureCount == 0)
            {
                // All rows succeeded
                return Ok(new ApiResponse<DefenseSessionImportResultDto>
                {
                    Code = ResponseCodes.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            else if (result.SuccessCount > 0)
            {
                // Partial success - return HTTP 207 Multi-Status
                return StatusCode(207, new ApiResponse<DefenseSessionImportResultDto>
                {
                    Code = ResponseCodes.MultiStatus,
                    Message = result.Message,
                    Data = result
                });
            }
            else
            {
                // All failed - return HTTP 400 Bad Request
                return BadRequest(new ApiResponse<DefenseSessionImportResultDto>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = result.Message,
                    Data = result
                });
            }
        }

        /// <summary>
        /// Download Excel template for defense session import (Admin và Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
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
