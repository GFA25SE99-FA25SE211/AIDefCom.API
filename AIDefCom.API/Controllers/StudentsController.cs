using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Dto.Student;
using AIDefCom.Service.Services.StudentService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all students");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<StudentReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Students"),
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Retrieving student with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            
            if (item == null)
            {
                _logger.LogWarning("Student with ID {Id} not found", id);
                throw new KeyNotFoundException($"Student with ID {id} not found");
            }

            return Ok(new ApiResponse<StudentReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Student"),
                Data = item
            });
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            _logger.LogInformation("Retrieving students for group ID: {GroupId}", groupId);
            var data = await _service.GetByGroupIdAsync(groupId);
            
            return Ok(new ApiResponse<IEnumerable<StudentReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Students by group"),
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StudentCreateDto dto)
        {
            _logger.LogInformation("Creating new student");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Student created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<StudentReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] StudentUpdateDto dto)
        {
            _logger.LogInformation("Updating student with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Student with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Student with ID {id} not found");
            }

            _logger.LogInformation("Student {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Student")
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Deleting student with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Student with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Student with ID {id} not found");
            }

            _logger.LogInformation("Student {Id} deleted successfully", id);
            return NoContent();
        }

        /// <summary>
        /// Import students from Excel file
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportStudents(IFormFile file)
        {
            _logger.LogInformation("Starting student import from Excel");

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = "File is required",
                    Data = null
                });
            }

            var result = await _service.ImportFromExcelAsync(file);

            _logger.LogInformation("Student import completed. Success: {Success}, Failures: {Failures}", 
                result.SuccessCount, result.FailureCount);

            return Ok(new ApiResponse<ImportResultDto>
            {
                Code = ResponseCodes.Success,
                Message = $"Import completed. {result.SuccessCount} students created successfully, {result.FailureCount} failed.",
                Data = result
            });
        }

        /// <summary>
        /// Download Excel template for student import
        /// </summary>
        [HttpGet("import/template")]
        public IActionResult DownloadTemplate()
        {
            _logger.LogInformation("Generating student import template");

            var fileBytes = _service.GenerateExcelTemplate();
            var fileName = $"Student_Import_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Download Excel template for student-group import
        /// </summary>
        [HttpGet("import/student-group-template")]
        public IActionResult DownloadStudentGroupTemplate()
        {
            _logger.LogInformation("Generating student-group import template");

            var fileBytes = _service.GenerateStudentGroupTemplate();
            var fileName = $"Student_Group_Import_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Import students with groups from Excel file
        /// </summary>
        [HttpPost("import/student-groups")]
        public async Task<IActionResult> ImportStudentsWithGroups([FromForm] StudentGroupImportRequestDto request)
        {
            _logger.LogInformation("Starting student-group import. SemesterId: {SemesterId}, MajorId: {MajorId}", request.SemesterId, request.MajorId);

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = "File is required",
                    Data = null
                });
            }

            if (request.SemesterId <= 0 || request.MajorId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = "Valid SemesterId and MajorId are required",
                    Data = null
                });
            }

            try
            {
                var result = await _service.ImportStudentsWithGroupsAsync(request.SemesterId, request.MajorId, request.File);

                _logger.LogInformation(
                    "Student-group import completed. Success: {Success}, Failures: {Failures}, Students: {Students}, Groups: {Groups}",
                    result.SuccessCount, result.FailureCount, result.CreatedStudentIds.Count, result.CreatedGroupIds.Count);

                return Ok(new ApiResponse<StudentGroupImportResultDto>
                {
                    Code = ResponseCodes.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during student-group import");
                return BadRequest(new ApiResponse<object>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
