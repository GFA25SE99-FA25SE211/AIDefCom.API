using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Services.DefenseSessionService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefenseSessionsController(IDefenseSessionService service, ILogger<DefenseSessionsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Defense sessions retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching defense sessions");
                return StatusCode(500, new { message = "Error retrieving defense sessions.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Defense session not found." });
            return Ok(new { message = "Defense session retrieved successfully.", data = item });
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            var data = await service.GetByGroupIdAsync(groupId);
            return Ok(new { message = "Defense sessions by group retrieved successfully.", data });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DefenseSessionCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Defense session created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DefenseSessionUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Defense session not found." });
            return Ok(new { message = "Defense session updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await service.DeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Defense session not found." });
            return Ok(new { message = "Defense session deleted successfully." });
        }
    }
}
