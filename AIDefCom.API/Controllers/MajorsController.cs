using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Major;
using AIDefCom.Service.Services.MajorService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/majors")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class MajorsController : ControllerBase
    {
        private readonly IMajorService _majorService;
        private readonly ILogger<MajorsController> _logger;

        public MajorsController(IMajorService majorService, ILogger<MajorsController> logger)
        {
            _majorService = majorService;
            _logger = logger;
        }

        /// <summary>
        /// Get all majors (Admin and Moderator)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all majors (includeDeleted: {IncludeDeleted})", includeDeleted);
            var majors = await _majorService.GetAllAsync(includeDeleted);
            
            return Ok(new ApiResponse<IEnumerable<MajorReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Majors"),
                Data = majors
            });
        }

        /// <summary>
        /// Get major by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving major with ID: {Id}", id);
            var major = await _majorService.GetByIdAsync(id);
            
            if (major == null)
            {
                _logger.LogWarning("Major with ID {Id} not found", id);
                throw new KeyNotFoundException($"Major with ID {id} not found");
            }

            return Ok(new ApiResponse<MajorReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Major"),
                Data = major
            });
        }

        /// <summary>
        /// Create a new major
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MajorCreateDto request)
        {
            _logger.LogInformation("Creating new major: {MajorName}", request.MajorName);
            var id = await _majorService.AddAsync(request);
            var created = await _majorService.GetByIdAsync(id);
            _logger.LogInformation("Major created with ID: {Id}", id);

            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<MajorReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing major
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MajorUpdateDto request)
        {
            _logger.LogInformation("Updating major with ID: {Id}", id);
            var success = await _majorService.UpdateAsync(id, request);
            
            if (!success)
            {
                _logger.LogWarning("Major with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Major with ID {id} not found");
            }

            _logger.LogInformation("Major {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Major")
            });
        }

        /// <summary>
        /// Soft delete a major (sets IsDeleted = true)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft deleting major with ID: {Id}", id);
            var success = await _majorService.SoftDeleteAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Major with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Major with ID {id} not found");
            }

            _logger.LogInformation("Major {Id} soft deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.SoftDeleted, "Major")
            });
        }

        /// <summary>
        /// Restore a soft-deleted major
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring major with ID: {Id}", id);
            var success = await _majorService.RestoreAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Major with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Major with ID {id} not found");
            }

            _logger.LogInformation("Major {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Major")
            });
        }
    }
}
