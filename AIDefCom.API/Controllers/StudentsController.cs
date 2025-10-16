using AIDefCom.Service.Dto.Student;
using AIDefCom.Service.Services.StudentService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController(IStudentService service, ILogger<StudentsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await service.GetAllAsync();
                return Ok(new { message = "Students retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching students");
                return StatusCode(500, new { message = "Error retrieving students.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var item = await service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Student not found." });
            return Ok(new { message = "Student retrieved successfully.", data = item });
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            var data = await service.GetByGroupIdAsync(groupId);
            return Ok(new { message = "Students by group retrieved successfully.", data });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StudentCreateDto dto)
        {
            var id = await service.AddAsync(dto);
            var created = await service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Student created successfully.", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] StudentUpdateDto dto)
        {
            var ok = await service.UpdateAsync(id, dto);
            if (!ok)
                return NotFound(new { message = "Student not found." });
            return Ok(new { message = "Student updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var ok = await service.DeleteAsync(id);
            if (!ok)
                return NotFound(new { message = "Student not found." });
            return Ok(new { message = "Student deleted successfully." });
        }
    }
}
