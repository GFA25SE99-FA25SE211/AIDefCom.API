using AIDefCom.Service.Dto.ProjectTask;
using AIDefCom.Service.Services.ProjectTaskService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTasksController(IProjectTaskService service, ILogger<ProjectTasksController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Project tasks retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching project tasks");
                return StatusCode(500, new { message = "Error retrieving project tasks.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await service.GetByIdAsync(id);
            if (task == null)
                return NotFound(new { message = "Project task not found." });
            return Ok(new { message = "Project task retrieved successfully.", data = task });
        }

        [HttpGet("assigner/{assignedById}")]
        public async Task<IActionResult> GetByAssigner(string assignedById)
        {
            var data = await service.GetByAssignerAsync(assignedById);
            return Ok(new { message = "Tasks assigned by user retrieved successfully.", data });
        }

        [HttpGet("assignee/{assignedToId}")]
        public async Task<IActionResult> GetByAssignee(string assignedToId)
        {
            var data = await service.GetByAssigneeAsync(assignedToId);
            return Ok(new { message = "Tasks assigned to user retrieved successfully.", data });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProjectTaskCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Project task created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectTaskUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Project task not found." });
            return Ok(new { message = "Project task updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await service.DeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Project task not found." });
            return Ok(new { message = "Project task deleted successfully." });
        }
    }
}
