using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.ProjectTask;
using AIDefCom.Service.Services.ProjectTaskService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    [Route("api/project-tasks")]
    [ApiController]
    public class ProjectTasksController : ControllerBase
    {
        private readonly IProjectTaskService _service;
        private readonly ILogger<ProjectTasksController> _logger;

        public ProjectTasksController(IProjectTaskService service, ILogger<ProjectTasksController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all project tasks");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<ProjectTaskReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Project tasks"),
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving project task with ID: {Id}", id);
            var task = await _service.GetByIdAsync(id);
            
            if (task == null)
            {
                _logger.LogWarning("Project task with ID {Id} not found", id);
                throw new KeyNotFoundException($"Project task with ID {id} not found");
            }

            return Ok(new ApiResponse<ProjectTaskReadDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Project task"),
                Data = task
            });
        }

        [HttpGet("assigner/{assignedById}")]
        public async Task<IActionResult> GetByAssigner(string assignedById)
        {
            _logger.LogInformation("Retrieving tasks assigned by user: {AssignedById}", assignedById);
            var data = await _service.GetByAssignerAsync(assignedById);
            
            return Ok(new ApiResponse<IEnumerable<ProjectTaskReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Tasks assigned by user"),
                Data = data
            });
        }

        [HttpGet("assignee/{assignedToId}")]
        public async Task<IActionResult> GetByAssignee(string assignedToId)
        {
            _logger.LogInformation("Retrieving tasks assigned to user: {AssignedToId}", assignedToId);
            var data = await _service.GetByAssigneeAsync(assignedToId);
            
            return Ok(new ApiResponse<IEnumerable<ProjectTaskReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Tasks assigned to user"),
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProjectTaskCreateDto dto)
        {
            _logger.LogInformation("Creating new project task");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Project task created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<ProjectTaskReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectTaskUpdateDto dto)
        {
            _logger.LogInformation("Updating project task with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Project task with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Project task with ID {id} not found");
            }

            _logger.LogInformation("Project task {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Project task")
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting project task with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Project task with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Project task with ID {id} not found");
            }

            _logger.LogInformation("Project task {Id} deleted successfully", id);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.Deleted, "Project task")
            });
        }
    }
}
