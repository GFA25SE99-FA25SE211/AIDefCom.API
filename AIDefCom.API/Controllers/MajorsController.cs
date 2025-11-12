using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Major;
using AIDefCom.Service.Services.MajorService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing majors
    /// </summary>
    [Route("api/majors")]
    [ApiController]
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
        /// Get all majors
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all majors");
            var majors = await _majorService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<MajorReadDto>>
            {
                MessageCode = MessageCodes.Major_Success0001,
                Message = SystemMessages.Major_Success0001,
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
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Major_Fail0001,
                    Message = SystemMessages.Major_Fail0001
                });
            }

            return Ok(new ApiResponse<MajorReadDto>
            {
                MessageCode = MessageCodes.Major_Success0002,
                Message = SystemMessages.Major_Success0002,
                Data = major
            });
        }

        /// <summary>
        /// Create a new major
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MajorCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Creating new major: {MajorName}", request.MajorName);
            var id = await _majorService.AddAsync(request);
            var created = await _majorService.GetByIdAsync(id);
            _logger.LogInformation("Major created with ID: {Id}", id);

            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<MajorReadDto>
            {
                MessageCode = MessageCodes.Major_Success0003,
                Message = SystemMessages.Major_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing major
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MajorUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Updating major with ID: {Id}", id);
            var success = await _majorService.UpdateAsync(id, request);
            if (!success)
            {
                _logger.LogWarning("Major with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Major_Fail0001,
                    Message = SystemMessages.Major_Fail0001
                });
            }

            _logger.LogInformation("Major {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Major_Success0004,
                Message = SystemMessages.Major_Success0004
            });
        }

        /// <summary>
        /// Delete a major
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting major with ID: {Id}", id);
            var success = await _majorService.DeleteAsync(id);
            if (!success)
            {
                _logger.LogWarning("Major with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Major_Fail0001,
                    Message = SystemMessages.Major_Fail0001
                });
            }

            _logger.LogInformation("Major {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
