using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.MajorRubric;
using AIDefCom.Service.Services.MajorRubricService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing major-rubric associations
    /// </summary>
    [Route("api/major-rubrics")]
    [ApiController]
    public class MajorRubricsController : ControllerBase
    {
        private readonly IMajorRubricService _service;
        private readonly ILogger<MajorRubricsController> _logger;

        public MajorRubricsController(IMajorRubricService service, ILogger<MajorRubricsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all major-rubric links
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all major-rubric links");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<MajorRubricReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Major-rubric links"),
                Data = data
            });
        }

        /// <summary>
        /// Get rubrics by major ID
        /// </summary>
        [HttpGet("major/{majorId}")]
        public async Task<IActionResult> GetByMajorId(int majorId)
        {
            _logger.LogInformation("Retrieving rubrics for major ID: {MajorId}", majorId);
            var data = await _service.GetByMajorIdAsync(majorId);
            
            return Ok(new ApiResponse<IEnumerable<MajorRubricReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Rubrics for major"),
                Data = data
            });
        }

        /// <summary>
        /// Get majors by rubric ID
        /// </summary>
        [HttpGet("rubric/{rubricId}")]
        public async Task<IActionResult> GetByRubricId(int rubricId)
        {
            _logger.LogInformation("Retrieving majors for rubric ID: {RubricId}", rubricId);
            var data = await _service.GetByRubricIdAsync(rubricId);
            
            return Ok(new ApiResponse<IEnumerable<MajorRubricReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Majors for rubric"),
                Data = data
            });
        }

        /// <summary>
        /// Create a new major-rubric link
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MajorRubricCreateDto dto)
        {
            _logger.LogInformation("Creating new major-rubric link for Major {MajorId} and Rubric {RubricId}", dto.MajorId, dto.RubricId);
            var id = await _service.AddAsync(dto);
            _logger.LogInformation("Major-rubric link created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetAll), new { id }, new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = new { id }
            });
        }

        /// <summary>
        /// Update an existing major-rubric link
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MajorRubricUpdateDto dto)
        {
            _logger.LogInformation("Updating major-rubric link with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Major-rubric link with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Major-rubric link with ID {id} not found");
            }

            _logger.LogInformation("Major-rubric link {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Major-rubric link")
            });
        }

        /// <summary>
        /// Delete a major-rubric link
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting major-rubric link with ID: {Id}", id);
            var success = await _service.DeleteAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Major-rubric link with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Major-rubric link with ID {id} not found");
            }

            _logger.LogInformation("Major-rubric link {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
