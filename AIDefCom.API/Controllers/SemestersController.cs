using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Semester;
using AIDefCom.Service.Services.SemesterService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing semesters
    /// </summary>
    [Route("api/semesters")]
    [ApiController]
    public class SemestersController : ControllerBase
    {
        private readonly ISemesterService _service;
        private readonly ILogger<SemestersController> _logger;

        public SemestersController(ISemesterService service, ILogger<SemestersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all semesters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all semesters (includeDeleted: {IncludeDeleted})", includeDeleted);
            var data = await _service.GetAllAsync(includeDeleted);
            
            return Ok(new ApiResponse<IEnumerable<SemesterReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Semesters"),
                Data = data
            });
        }

        /// <summary>
        /// Get semester by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving semester with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            
            if (item == null)
            {
                _logger.LogWarning("Semester with ID {Id} not found", id);
                throw new KeyNotFoundException($"Semester with ID {id} not found");
            }

            return Ok(new ApiResponse<SemesterReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Semester"),
                Data = item
            });
        }

        /// <summary>
        /// Get semesters by major ID
        /// </summary>
        [HttpGet("major/{majorId}")]
        public async Task<IActionResult> GetByMajorId(int majorId)
        {
            _logger.LogInformation("Retrieving semesters for major ID: {MajorId}", majorId);
            var data = await _service.GetByMajorIdAsync(majorId);
            
            return Ok(new ApiResponse<IEnumerable<SemesterReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Semesters by major"),
                Data = data
            });
        }

        /// <summary>
        /// Create a new semester
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SemesterCreateDto dto)
        {
            _logger.LogInformation("Creating new semester: {SemesterName}", dto.SemesterName);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Semester created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<SemesterReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing semester
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SemesterUpdateDto dto)
        {
            _logger.LogInformation("Updating semester with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Semester with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Semester with ID {id} not found");
            }

            _logger.LogInformation("Semester {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Semester")
            });
        }

        /// <summary>
        /// Soft delete a semester (sets IsDeleted = true)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft deleting semester with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Semester with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Semester with ID {id} not found");
            }

            _logger.LogInformation("Semester {Id} soft deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.SoftDeleted, "Semester")
            });
        }

        /// <summary>
        /// Restore a soft-deleted semester
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring semester with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Semester with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Semester with ID {id} not found");
            }

            _logger.LogInformation("Semester {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Semester")
            });
        }
    }
}
