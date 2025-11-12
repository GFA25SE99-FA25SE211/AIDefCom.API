using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Student;
using AIDefCom.Service.Services.StudentService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing students
    /// </summary>
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService service, ILogger<StudentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all students
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all students");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<StudentReadDto>>
            {
                MessageCode = MessageCodes.Student_Success0001,
                Message = SystemMessages.Student_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get student by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Retrieving student with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Student with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Student_Fail0001,
                    Message = SystemMessages.Student_Fail0001
                });
            }

            return Ok(new ApiResponse<StudentReadDto>
            {
                MessageCode = MessageCodes.Student_Success0002,
                Message = SystemMessages.Student_Success0002,
                Data = item
            });
        }

        /// <summary>
        /// Get students by group ID
        /// </summary>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            _logger.LogInformation("Retrieving students for group ID: {GroupId}", groupId);
            var data = await _service.GetByGroupIdAsync(groupId);
            return Ok(new ApiResponse<IEnumerable<StudentReadDto>>
            {
                MessageCode = MessageCodes.Student_Success0006,
                Message = SystemMessages.Student_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Create a new student
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StudentCreateDto dto)
        {
            _logger.LogInformation("Creating new student");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Student created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<StudentReadDto>
            {
                MessageCode = MessageCodes.Student_Success0003,
                Message = SystemMessages.Student_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing student
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] StudentUpdateDto dto)
        {
            _logger.LogInformation("Updating student with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Student with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Student_Fail0001,
                    Message = SystemMessages.Student_Fail0001
                });
            }

            _logger.LogInformation("Student {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Student_Success0004,
                Message = SystemMessages.Student_Success0004
            });
        }

        /// <summary>
        /// Delete a student
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Deleting student with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Student with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Student_Fail0001,
                    Message = SystemMessages.Student_Fail0001
                });
            }

            _logger.LogInformation("Student {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
