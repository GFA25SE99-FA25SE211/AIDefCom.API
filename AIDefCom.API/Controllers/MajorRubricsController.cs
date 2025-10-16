using AIDefCom.Service.Dto.MajorRubric;
using AIDefCom.Service.Services.MajorRubricService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MajorRubricsController(IMajorRubricService service, ILogger<MajorRubricsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Major–Rubric links retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all MajorRubric links");
                return StatusCode(500, new { message = "Error retrieving data.", error = ex.Message });
            }
        }

        [HttpGet("major/{majorId}")]
        public async Task<IActionResult> GetByMajorId(int majorId)
        {
            try
            {
                var data = await service.GetByMajorIdAsync(majorId);
                return Ok(new { message = "Rubrics for Major retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching rubrics for major {MajorId}", majorId);
                return StatusCode(500, new { message = "Error retrieving rubrics for major.", error = ex.Message });
            }
        }

        [HttpGet("rubric/{rubricId}")]
        public async Task<IActionResult> GetByRubricId(int rubricId)
        {
            try
            {
                var data = await service.GetByRubricIdAsync(rubricId);
                return Ok(new { message = "Majors for Rubric retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching majors for rubric {RubricId}", rubricId);
                return StatusCode(500, new { message = "Error retrieving majors for rubric.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MajorRubricCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var id = await service.AddAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id }, new { message = "Major–Rubric link created successfully.", id });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "MajorRubric pair already exists");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding MajorRubric link");
                return StatusCode(500, new { message = "Error adding MajorRubric link.", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MajorRubricUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var ok = await service.UpdateAsync(id, dto);
                if (!ok) return NotFound(new { message = "Major–Rubric link not found." });
                return Ok(new { message = "Major–Rubric link updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Duplicate Major–Rubric pair on update (Id={Id})", id);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating MajorRubric link {Id}", id);
                return StatusCode(500, new { message = "Error updating link.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await service.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Major–Rubric link not found." });

                return Ok(new { message = "Major–Rubric link deleted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting MajorRubric link {Id}", id);
                return StatusCode(500, new { message = "Error deleting link.", error = ex.Message });
            }
        }
    }
}
