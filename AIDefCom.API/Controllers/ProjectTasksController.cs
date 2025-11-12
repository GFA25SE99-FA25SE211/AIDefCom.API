using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.ProjectTask;
using AIDefCom.Service.Services.ProjectTaskService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing project tasks
    /// </summary>
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

        /// <summary>
        /// Get all project tasks
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all project tasks");
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<ProjectTaskReadDto>>
            {
                MessageCode = MessageCodes.ProjectTask_Success0001,
                Message = SystemMessages.ProjectTask_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get project task by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving project task with ID: {Id}", id);
            var task = await _service.GetByIdAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Project task with ID {Id} not found", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.ProjectTask_Fail0001,
                    Message = SystemMessages.ProjectTask_Fail0001
                });
            }

            return Ok(new ApiResponse<ProjectTaskReadDto>
            {
                MessageCode = MessageCodes.ProjectTask_Success0002,
                Message = SystemMessages.ProjectTask_Success0002,
                Data = task
            });
        }

        /// <summary>
        /// Get tasks by assigner ID
        /// </summary>
        [HttpGet("assigner/{assignedById}")]
        public async Task<IActionResult> GetByAssigner(string assignedById)
        {
            _logger.LogInformation("Retrieving tasks assigned by user: {AssignedById}", assignedById);
            var data = await _service.GetByAssignerAsync(assignedById);
            return Ok(new ApiResponse<IEnumerable<ProjectTaskReadDto>>
            {
                MessageCode = MessageCodes.ProjectTask_Success0006,
                Message = SystemMessages.ProjectTask_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Get tasks by assignee ID
        /// </summary>
        [HttpGet("assignee/{assignedToId}")]
        public async Task<IActionResult> GetByAssignee(string assignedToId)
        {
            _logger.LogInformation("Retrieving tasks assigned to user: {AssignedToId}", assignedToId);
            var data = await _service.GetByAssigneeAsync(assignedToId);
            return Ok(new ApiResponse<IEnumerable<ProjectTaskReadDto>>
            {
                MessageCode = MessageCodes.ProjectTask_Success0007,
                Message = SystemMessages.ProjectTask_Success0007,
                Data = data
            });
        }

        /// <summary>
        /// Create a new project task
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProjectTaskCreateDto dto)
        {
            _logger.LogInformation("Creating new project task");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Project task created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<ProjectTaskReadDto>
            {
                MessageCode = MessageCodes.ProjectTask_Success0003,
                Message = SystemMessages.ProjectTask_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing project task
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectTaskUpdateDto dto)
        {
            _logger.LogInformation("Updating project task with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                _logger.LogWarning("Project task with ID {Id} not found for update", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.ProjectTask_Fail0001,
                    Message = SystemMessages.ProjectTask_Fail0001
                });
            }

            _logger.LogInformation("Project task {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.ProjectTask_Success0004,
                Message = SystemMessages.ProjectTask_Success0004
            });
        }

        /// <summary>
        /// Delete a project task
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting project task with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                _logger.LogWarning("Project task with ID {Id} not found for deletion", id);
                return NotFound(new ApiResponse<object>
                {
                    MessageCode = MessageCodes.ProjectTask_Fail0001,
                    Message = SystemMessages.ProjectTask_Fail0001
                });
            }

            _logger.LogInformation("Project task {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
