using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Rubric;
using AIDefCom.Service.Services.RubricService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing rubrics
    /// </summary>
    [Route("api/rubrics")]
    [ApiController]
    public class RubricsController : ControllerBase
    {
        private readonly IRubricService _rubricService;
        private readonly ILogger<RubricsController> _logger;

        public RubricsController(IRubricService rubricService, ILogger<RubricsController> logger)
        {
            _rubricService = rubricService;
            _logger = logger;
        }

        /// <summary>
        /// Get all rubrics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all rubrics");
            var rubrics = await _rubricService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<RubricReadDto>>
            {
                MessageCode = MessageCodes.Rubric_Success0001,
                Message = SystemMessages.Rubric_Success0001,
                Data = rubrics
            });
        }

        /// <summary>
        /// Get rubric by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving rubric with ID: {Id}", id);
            var rubric = await _rubricService.GetByIdAsync(id);
            if (rubric == null)
            {
                _logger.LogWarning("Rubric with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Rubric_Fail0001,
                    Message = SystemMessages.Rubric_Fail0001
                });
            }

            return Ok(new ApiResponse<RubricReadDto>
            {
                MessageCode = MessageCodes.Rubric_Success0002,
                Message = SystemMessages.Rubric_Success0002,
                Data = rubric
            });
        }

        /// <summary>
        /// Create a new rubric
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RubricCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Creating new rubric: {RubricName}", request.RubricName);
            var id = await _rubricService.AddAsync(request);
            var createdRubric = await _rubricService.GetByIdAsync(id);
            _logger.LogInformation("Rubric created with ID: {Id}", id);

            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<RubricReadDto>
            {
                MessageCode = MessageCodes.Rubric_Success0003,
                Message = SystemMessages.Rubric_Success0003,
                Data = createdRubric
            });
        }

        /// <summary>
        /// Update an existing rubric
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RubricUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.General_Validation0001,
                    Message = SystemMessages.General_Validation0001,
                    Data = ModelState
                });

            _logger.LogInformation("Updating rubric with ID: {Id}", id);
            var success = await _rubricService.UpdateAsync(id, request);
            if (!success)
            {
                _logger.LogWarning("Rubric with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Rubric_Fail0001,
                    Message = SystemMessages.Rubric_Fail0001
                });
            }

            _logger.LogInformation("Rubric {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Rubric_Success0004,
                Message = SystemMessages.Rubric_Success0004
            });
        }

        /// <summary>
        /// Delete a rubric
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting rubric with ID: {Id}", id);
            var success = await _rubricService.DeleteAsync(id);
            if (!success)
            {
                _logger.LogWarning("Rubric with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.Rubric_Fail0001,
                    Message = SystemMessages.Rubric_Fail0001
                });
            }

            _logger.LogInformation("Rubric {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
