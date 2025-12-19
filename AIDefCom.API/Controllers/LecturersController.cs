using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Lecturer;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Services.LecturerService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing lecturers
    /// </summary>
    [Route("api/lecturers")]
    [ApiController]
    public class LecturersController : ControllerBase
    {
        private readonly ILecturerService _service;
        private readonly ILogger<LecturersController> _logger;

        public LecturersController(ILecturerService service, ILogger<LecturersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all lecturers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all lecturers");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<LecturerReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Lecturers"),
                Data = data
            });
        }

        /// <summary>
        /// Get lecturer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Retrieving lecturer with ID: {Id}", id);
            var item = await _service.GetByIdAsync(id);
            
            if (item == null)
            {
                _logger.LogWarning("Lecturer with ID {Id} not found", id);
                throw new KeyNotFoundException($"Lecturer with ID {id} not found");
            }

            return Ok(new ApiResponse<LecturerReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Lecturer"),
                Data = item
            });
        }

        /// <summary>
        /// Get lecturers by department
        /// </summary>
        [HttpGet("department/{department}")]
        public async Task<IActionResult> GetByDepartment(string department)
        {
            _logger.LogInformation("Retrieving lecturers for department: {Department}", department);
            var data = await _service.GetByDepartmentAsync(department);
            
            return Ok(new ApiResponse<IEnumerable<LecturerReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Lecturers by department"),
                Data = data
            });
        }

        /// <summary>
        /// Get lecturers by academic rank
        /// </summary>
        [HttpGet("rank/{academicRank}")]
        public async Task<IActionResult> GetByAcademicRank(string academicRank)
        {
            _logger.LogInformation("Retrieving lecturers for academic rank: {AcademicRank}", academicRank);
            var data = await _service.GetByAcademicRankAsync(academicRank);
            
            return Ok(new ApiResponse<IEnumerable<LecturerReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Lecturers by academic rank"),
                Data = data
            });
        }

        /// <summary>
        /// Create a new lecturer
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] LecturerCreateDto dto)
        {
            _logger.LogInformation("Creating new lecturer");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Lecturer created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<LecturerReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing lecturer
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] LecturerUpdateDto dto)
        {
            _logger.LogInformation("Updating lecturer with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Lecturer with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Lecturer with ID {id} not found");
            }

            _logger.LogInformation("Lecturer {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Lecturer")
            });
        }

        /// <summary>
        /// Delete a lecturer
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Deleting lecturer with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Lecturer with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Lecturer with ID {id} not found");
            }

            _logger.LogInformation("Lecturer {Id} deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.Deleted, "Lecturer")
            });
        }

        /// <summary>
        /// Import lecturers from Excel file
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportLecturers(IFormFile file)
        {
            _logger.LogInformation("Starting lecturer import from Excel");

            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file), "File is required");
            }

            var result = await _service.ImportFromExcelAsync(file);

            _logger.LogInformation("Lecturer import completed. Success: {Success}, Failures: {Failures}", 
                result.SuccessCount, result.FailureCount);
            var msg = $"Import completed. {result.SuccessCount} lecturers created successfully, {result.FailureCount} failed.";

            // ✅ Return different HTTP status codes based on import results
            // HTTP 200: All success
            // HTTP 207: Partial success (some rows succeeded, some failed)
            // HTTP 400: All failed

            if (result.FailureCount == 0)
            {
                // All rows succeeded
                return Ok(new ApiResponse<ImportResultDto>
                {
                    Code = ResponseCodes.Success,
                    Message = msg,
                    Data = result
                });
            }
            else if (result.SuccessCount > 0)
            {
                // Partial success - return HTTP 207 Multi-Status
                return StatusCode(207, new ApiResponse<ImportResultDto>
                {
                    Code = ResponseCodes.MultiStatus,
                    Message = msg,
                    Data = result
                });
            }
            else
            {
                // All failed - return HTTP 400 Bad Request
                return BadRequest(new ApiResponse<ImportResultDto>
                {
                    Code = ResponseCodes.BadRequest,
                    Message = msg,
                    Data = result
                });
            }
        }

        /// <summary>
        /// Download Excel template for lecturer import
        /// </summary>
        [HttpGet("import/template")]
        public IActionResult DownloadTemplate()
        {
            _logger.LogInformation("Generating lecturer import template");

            var fileBytes = _service.GenerateExcelTemplate();
            var fileName = $"Lecturer_Import_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
