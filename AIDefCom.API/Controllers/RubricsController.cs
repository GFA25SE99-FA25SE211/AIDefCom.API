using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Rubric;
using AIDefCom.Service.Services.RubricService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/rubrics")]
    [ApiController]
    [Authorize] // Tất cả endpoints yêu cầu authenticated
    public class RubricsController : ControllerBase
    {
        private readonly IRubricService _rubricService;
        private readonly ILogger<RubricsController> _logger;

        public RubricsController(IRubricService rubricService, ILogger<RubricsController> logger)
        {
            _rubricService = rubricService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Retrieving all rubrics (includeDeleted: {IncludeDeleted})", includeDeleted);
            var rubrics = await _rubricService.GetAllAsync(includeDeleted);
            return Ok(new ApiResponse<IEnumerable<RubricReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Rubrics"),
                Data = rubrics
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving rubric with ID: {Id}", id);
            var rubric = await _rubricService.GetByIdAsync(id);
            if (rubric == null)
            {
                _logger.LogWarning("Rubric with ID {Id} not found", id);
                throw new KeyNotFoundException($"Rubric with ID {id} not found");
            }
            return Ok(new ApiResponse<RubricReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Rubric"),
                Data = rubric
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RubricCreateDto request)
        {
            _logger.LogInformation("Creating new rubric: {RubricName}", request.RubricName);
            var id = await _rubricService.AddAsync(request);
            var createdRubric = await _rubricService.GetByIdAsync(id);
            _logger.LogInformation("Rubric created with ID: {Id}", id);
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<RubricReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = createdRubric
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RubricUpdateDto request)
        {
            _logger.LogInformation("Updating rubric with ID: {Id}", id);
            var success = await _rubricService.UpdateAsync(id, request);
            if (!success)
            {
                _logger.LogWarning("Rubric with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Rubric with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Rubric")
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft deleting rubric with ID: {Id}", id);
            var success = await _rubricService.SoftDeleteAsync(id);
            if (!success)
            {
                _logger.LogWarning("Rubric with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Rubric with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.SoftDeleted, "Rubric")
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring rubric with ID: {Id}", id);
            var success = await _rubricService.RestoreAsync(id);
            if (!success)
            {
                _logger.LogWarning("Rubric with ID {Id} not found for restoration", id);
                throw new KeyNotFoundException($"Rubric with ID {id} not found");
            }
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Restored, "Rubric")
            });
        }
    }
}
