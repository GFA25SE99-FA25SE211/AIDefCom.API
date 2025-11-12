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
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all semesters");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<SemesterReadDto>>
            {
                MessageCode = MessageCodes.Semester_Success0001,
                Message = SystemMessages.Semester_Success0001,
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
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Semester_Fail0001,
                    Message = SystemMessages.Semester_Fail0001
                });
            }

            return Ok(new ApiResponse<SemesterReadDto>
            {
                MessageCode = MessageCodes.Semester_Success0002,
                Message = SystemMessages.Semester_Success0002,
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
                MessageCode = MessageCodes.Semester_Success0006,
                Message = SystemMessages.Semester_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Create a new semester
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SemesterCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Creating new semester: {SemesterName}", dto.SemesterName);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Semester created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<SemesterReadDto>
            {
                MessageCode = MessageCodes.Semester_Success0003,
                Message = SystemMessages.Semester_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing semester
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SemesterUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Updating semester with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Semester with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Semester_Fail0001,
                    Message = SystemMessages.Semester_Fail0001
                });
            }

            _logger.LogInformation("Semester {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Semester_Success0004,
                Message = SystemMessages.Semester_Success0004
            });
        }

        /// <summary>
        /// Delete a semester
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting semester with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Semester with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Semester_Fail0001,
                    Message = SystemMessages.Semester_Fail0001
                });
            }

            _logger.LogInformation("Semester {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
