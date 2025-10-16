using AIDefCom.Service.Dto.Report;
using AIDefCom.Service.Services.ReportService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController(IReportService service, ILogger<ReportsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Reports retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all reports");
                return StatusCode(500, new { message = "Error retrieving reports.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var report = await service.GetByIdAsync(id);
                if (report == null)
                    return NotFound(new { message = "Report not found." });

                return Ok(new { message = "Report retrieved successfully.", data = report });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching report {Id}", id);
                return StatusCode(500, new { message = "Error retrieving report.", error = ex.Message });
            }
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySessionId(int sessionId)
        {
            try
            {
                var data = await service.GetBySessionIdAsync(sessionId);
                return Ok(new { message = "Reports for session retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching reports for session {Id}", sessionId);
                return StatusCode(500, new { message = "Error retrieving reports for session.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ReportCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var id = await service.AddAsync(dto);
                var created = await service.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, new { message = "Report created successfully.", data = created });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding report");
                return StatusCode(500, new { message = "Error adding report.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReportUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input.", errors = ModelState });

            try
            {
                var success = await service.UpdateAsync(id, dto);
                if (!success)
                    return NotFound(new { message = "Report not found." });

                return Ok(new { message = "Report updated successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating report {Id}", id);
                return StatusCode(500, new { message = "Error updating report.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await service.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Report not found." });

                return Ok(new { message = "Report deleted successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting report {Id}", id);
                return StatusCode(500, new { message = "Error deleting report.", error = ex.Message });
            }
        }
    }
}
