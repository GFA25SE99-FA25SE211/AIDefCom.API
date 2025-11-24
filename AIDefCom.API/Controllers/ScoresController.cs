using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Score;
using AIDefCom.Service.Services.ScoreService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing scores (grading)
    /// </summary>
    [Route("api/scores")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IScoreService _service;
        private readonly ILogger<ScoresController> _logger;

        public ScoresController(IScoreService service, ILogger<ScoresController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all scores
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all scores");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<ScoreReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Scores"),
                Data = data
            });
        }

        /// <summary>
        /// Get score by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving score with ID: {Id}", id);
            var score = await _service.GetByIdAsync(id);
            
            if (score == null)
            {
                _logger.LogWarning("Score with ID {Id} not found", id);
                throw new KeyNotFoundException($"Score with ID {id} not found");
            }

            return Ok(new ApiResponse<ScoreReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Score"),
                Data = score
            });
        }

        /// <summary>
        /// Get scores by defense session ID
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySessionId(int sessionId)
        {
            _logger.LogInformation("Retrieving scores for session ID: {SessionId}", sessionId);
            var data = await _service.GetBySessionIdAsync(sessionId);
            
            return Ok(new ApiResponse<IEnumerable<ScoreReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Scores by session"),
                Data = data
            });
        }

        /// <summary>
        /// Get scores by student ID
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(string studentId)
        {
            _logger.LogInformation("Retrieving scores for student ID: {StudentId}", studentId);
            var data = await _service.GetByStudentIdAsync(studentId);
            
            return Ok(new ApiResponse<IEnumerable<ScoreReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Scores by student"),
                Data = data
            });
        }

        /// <summary>
        /// Get scores by evaluator (lecturer) ID
        /// </summary>
        [HttpGet("evaluator/{evaluatorId}")]
        public async Task<IActionResult> GetByEvaluatorId(string evaluatorId)
        {
            _logger.LogInformation("Retrieving scores for evaluator ID: {EvaluatorId}", evaluatorId);
            var data = await _service.GetByEvaluatorIdAsync(evaluatorId);
            
            return Ok(new ApiResponse<IEnumerable<ScoreReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Scores by evaluator"),
                Data = data
            });
        }

        /// <summary>
        /// Get scores by rubric ID
        /// </summary>
        [HttpGet("rubric/{rubricId}")]
        public async Task<IActionResult> GetByRubricId(int rubricId)
        {
            _logger.LogInformation("Retrieving scores for rubric ID: {RubricId}", rubricId);
            var data = await _service.GetByRubricIdAsync(rubricId);
            
            return Ok(new ApiResponse<IEnumerable<ScoreReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Scores by rubric"),
                Data = data
            });
        }

        /// <summary>
        /// Create a new score
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ScoreCreateDto dto)
        {
            _logger.LogInformation("Creating new score for student {StudentId} by evaluator {EvaluatorId}", dto.StudentId, dto.EvaluatorId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Score created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<ScoreReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing score
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ScoreUpdateDto dto)
        {
            _logger.LogInformation("Updating score with ID: {Id}", id);
            var success = await _service.UpdateAsync(id, dto);
            
            if (!success)
            {
                _logger.LogWarning("Score with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Score with ID {id} not found");
            }

            _logger.LogInformation("Score {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Score")
            });
        }

        /// <summary>
        /// Delete a score
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting score with ID: {Id}", id);
            var success = await _service.DeleteAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Score with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Score with ID {id} not found");
            }

            _logger.LogInformation("Score {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
