using AIDefCom.Service.Dto.MemberNote;
using AIDefCom.Service.Services.MemberNoteService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberNotesController(IMemberNoteService service, ILogger<MemberNotesController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Member notes retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching member notes");
                return StatusCode(500, new { message = "Error retrieving member notes.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var note = await service.GetByIdAsync(id);
            if (note == null)
                return NotFound(new { message = "Member note not found." });
            return Ok(new { message = "Member note retrieved successfully.", data = note });
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            var data = await service.GetByGroupIdAsync(groupId);
            return Ok(new { message = "Member notes by group retrieved successfully.", data });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var data = await service.GetByUserIdAsync(userId);
            return Ok(new { message = "Member notes by user retrieved successfully.", data });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MemberNoteCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Member note created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MemberNoteUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Member note not found." });
            return Ok(new { message = "Member note updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await service.DeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Member note not found." });
            return Ok(new { message = "Member note deleted successfully." });
        }
    }
}
