using AIDefCom.Service.Dto.Council;
using AIDefCom.Service.Services.CouncilService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouncilsController(ICouncilService service, ILogger<CouncilsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                var data = await service.GetAllAsync(includeInactive);
                return Ok(new { message = "Councils retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching councils");
                return StatusCode(500, new { message = "Error retrieving councils.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entity = await service.GetByIdAsync(id);
            if (entity == null)
                return NotFound(new { message = "Council not found." });
            return Ok(new { message = "Council retrieved successfully.", data = entity });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CouncilCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Council created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CouncilUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Council not found." });
            return Ok(new { message = "Council updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var ok = await service.SoftDeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Council not found." });
            return Ok(new { message = "Council deactivated (soft deleted) successfully." });
        }

        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var ok = await service.RestoreAsync(id);
            if (!ok)
                return NotFound(new { message = "Council not found or already active." });
            return Ok(new { message = "Council restored successfully." });
        }
    }
}
