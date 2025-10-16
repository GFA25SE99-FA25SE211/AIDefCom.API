using AIDefCom.Service.Dto.CommitteeAssignment;
using AIDefCom.Service.Services.CommitteeAssignmentService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommitteeAssignmentsController(ICommitteeAssignmentService service, ILogger<CommitteeAssignmentsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Committee assignments retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching committee assignments");
                return StatusCode(500, new { message = "Error retrieving committee assignments.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Committee assignment not found." });
            return Ok(new { message = "Committee assignment retrieved successfully.", data = item });
        }

        [HttpGet("council/{councilId}")]
        public async Task<IActionResult> GetByCouncilId(int councilId)
        {
            var data = await service.GetByCouncilIdAsync(councilId);
            return Ok(new { message = "Committee assignments by council retrieved successfully.", data });
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySessionId(int sessionId)
        {
            var data = await service.GetBySessionIdAsync(sessionId);
            return Ok(new { message = "Committee assignments by session retrieved successfully.", data });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var data = await service.GetByUserIdAsync(userId);
            return Ok(new { message = "Committee assignments by user retrieved successfully.", data });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CommitteeAssignmentCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Committee assignment created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommitteeAssignmentUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Committee assignment not found." });
            return Ok(new { message = "Committee assignment updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await service.DeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Committee assignment not found." });
            return Ok(new { message = "Committee assignment deleted successfully." });
        }
    }
}
