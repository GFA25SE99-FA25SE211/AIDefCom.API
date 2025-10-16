using AIDefCom.Service.Dto.Major;
using AIDefCom.Service.Services.MajorService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MajorsController(IMajorService majorService, ILogger<MajorsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var majors = await majorService.GetAllAsync();
                return Ok(new { message = "Majors retrieved successfully.", data = majors });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all majors");
                return StatusCode(500, new { message = "An error occurred while retrieving majors.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var major = await majorService.GetByIdAsync(id);
                if (major == null)
                    return NotFound(new { message = "Major not found." });

                return Ok(new { message = "Major retrieved successfully.", data = major });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching major with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the major.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MajorCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var id = await majorService.AddAsync(request);
                var created = await majorService.GetByIdAsync(id);

                return CreatedAtAction(nameof(GetById), new { id }, new { message = "Major created successfully.", data = created });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Major name already exists: {Name}", request.MajorName);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding major");
                return StatusCode(500, new { message = "An error occurred while adding the major.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MajorUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var success = await majorService.UpdateAsync(id, request);
                if (!success)
                    return NotFound(new { message = "Major not found." });

                return Ok(new { message = "Major updated successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating major with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the major.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await majorService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Major not found." });

                return Ok(new { message = "Major deleted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting major with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the major.", error = ex.Message });
            }
        }
    }
}
