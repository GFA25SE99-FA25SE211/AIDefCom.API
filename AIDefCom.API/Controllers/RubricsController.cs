using AIDefCom.Service.Dto.Rubric;
using AIDefCom.Service.Services.RubricService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RubricsController(IRubricService rubricService, ILogger<RubricsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rubrics = await rubricService.GetAllAsync();
                return Ok(new
                {
                    message = "Rubrics retrieved successfully.",
                    data = rubrics
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all rubrics");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving rubrics.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var rubric = await rubricService.GetByIdAsync(id);
                if (rubric == null)
                {
                    return NotFound(new { message = "Rubric not found." });
                }

                return Ok(new
                {
                    message = "Rubric retrieved successfully.",
                    data = rubric
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching rubric with ID {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the rubric.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RubricCreateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input.",
                    errors = ModelState
                });
            }

            try
            {
                var id = await rubricService.AddAsync(request);
                var createdRubric = await rubricService.GetByIdAsync(id);

                return CreatedAtAction(nameof(GetById), new { id }, new
                {
                    message = "Rubric created successfully.",
                    data = createdRubric
                });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Rubric name already exists: {Name}", request.RubricName);
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding rubric");
                return StatusCode(500, new
                {
                    message = "An error occurred while adding the rubric.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RubricUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input.",
                    errors = ModelState
                });
            }

            try
            {
                var success = await rubricService.UpdateAsync(id, request);
                if (!success)
                {
                    return NotFound(new { message = "Rubric not found." });
                }

                return Ok(new { message = "Rubric updated successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating rubric with ID {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the rubric.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await rubricService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Rubric not found." });
                }

                return Ok(new { message = "Rubric deleted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting rubric with ID {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the rubric.",
                    error = ex.Message
                });
            }
        }
    }
}
