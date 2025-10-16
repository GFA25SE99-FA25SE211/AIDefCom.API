using AIDefCom.Service.Dto.Group;
using AIDefCom.Service.Services.GroupService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController(IGroupService service, ILogger<GroupsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Groups retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching groups");
                return StatusCode(500, new { message = "Error retrieving groups.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var group = await service.GetByIdAsync(id);
            if (group == null)
                return NotFound(new { message = "Group not found." });
            return Ok(new { message = "Group retrieved successfully.", data = group });
        }

        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetBySemesterId(int semesterId)
        {
            var data = await service.GetBySemesterIdAsync(semesterId);
            return Ok(new { message = "Groups by semester retrieved successfully.", data });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] GroupCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Group created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] GroupUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Group not found." });
            return Ok(new { message = "Group updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var ok = await service.DeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Group not found." });
            return Ok(new { message = "Group deleted successfully." });
        }
    }
}
