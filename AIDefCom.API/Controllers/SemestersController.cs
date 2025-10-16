using AIDefCom.Service.Dto.Semester;
using AIDefCom.Service.Services.SemesterService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemestersController(ISemesterService service, ILogger<SemestersController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Semesters retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching semesters");
                return StatusCode(500, new { message = "Error retrieving semesters.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await service.GetByIdAsync(id);
                if (item == null)
                    return NotFound(new { message = "Semester not found." });
                return Ok(new { message = "Semester retrieved successfully.", data = item });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching semester {Id}", id);
                return StatusCode(500, new { message = "Error retrieving semester.", error = ex.Message });
            }
        }

        [HttpGet("major/{majorId}")]
        public async Task<IActionResult> GetByMajorId(int majorId)
        {
            try
            {
                var data = await service.GetByMajorIdAsync(majorId);
                return Ok(new { message = "Semesters by major retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching semesters for major {MajorId}", majorId);
                return StatusCode(500, new { message = "Error retrieving semesters by major.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SemesterCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var id = await service.AddAsync(dto);
                var created = await service.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, new { message = "Semester created successfully.", data = created });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Duplicate semester detected");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding semester");
                return StatusCode(500, new { message = "Error adding semester.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SemesterUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var ok = await service.UpdateAsync(id, dto);
                if (!ok)
                    return NotFound(new { message = "Semester not found." });
                return Ok(new { message = "Semester updated successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating semester {Id}", id);
                return StatusCode(500, new { message = "Error updating semester.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await service.DeleteAsync(id);
                if (!ok)
                    return NotFound(new { message = "Semester not found." });
                return Ok(new { message = "Semester deleted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting semester {Id}", id);
                return StatusCode(500, new { message = "Error deleting semester.", error = ex.Message });
            }
        }
    }
}
