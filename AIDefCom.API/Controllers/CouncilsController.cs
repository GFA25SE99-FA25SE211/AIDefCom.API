using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Council;
using AIDefCom.Service.Services.CouncilService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing councils
    /// </summary>
    [Route("api/councils")]
    [ApiController]
    public class CouncilsController : ControllerBase
    {
        private readonly ICouncilService _service;
        private readonly ILogger<CouncilsController> _logger;

        public CouncilsController(ICouncilService service, ILogger<CouncilsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all councils
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            _logger.LogInformation("Retrieving all councils (includeInactive: {IncludeInactive})", includeInactive);
            var data = await _service.GetAllAsync(includeInactive);
            
            return Ok(new ApiResponse<IEnumerable<CouncilReadDto>>
            {
                MessageCode = MessageCodes.Council_Success0001,
                Message = SystemMessages.Council_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get council by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving council with ID: {Id}", id);
            var entity = await _service.GetByIdAsync(id);
            
            if (entity == null)
            {
                _logger.LogWarning("Council with ID {Id} not found", id);
                throw new KeyNotFoundException($"Council with ID {id} not found");
            }

            return Ok(new ApiResponse<CouncilReadDto>
            {
                MessageCode = MessageCodes.Council_Success0002,
                Message = SystemMessages.Council_Success0002,
                Data = entity
            });
        }

        /// <summary>
        /// Create a new council
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CouncilCreateDto dto)
        {
            _logger.LogInformation("Creating new council");
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Council created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<CouncilReadDto>
            {
                MessageCode = MessageCodes.Council_Success0003,
                Message = SystemMessages.Council_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing council
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CouncilUpdateDto dto)
        {
            _logger.LogInformation("Updating council with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Council with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Council with ID {id} not found");
            }

            _logger.LogInformation("Council {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Council_Success0004,
                Message = SystemMessages.Council_Success0004
            });
        }

        /// <summary>
        /// Soft delete a council
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            _logger.LogInformation("Soft deleting council with ID: {Id}", id);
            var ok = await _service.SoftDeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Council with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Council with ID {id} not found");
            }

            _logger.LogInformation("Council {Id} soft deleted successfully", id);
            return NoContent();
        }

        /// <summary>
        /// Restore a soft-deleted council
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Restoring council with ID: {Id}", id);
            var ok = await _service.RestoreAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Council with ID {Id} not found or already active for restoration", id);
                throw new KeyNotFoundException($"Council with ID {id} not found or already active");
            }

            _logger.LogInformation("Council {Id} restored successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.Council_Success0006,
                Message = SystemMessages.Council_Success0006
            });
        }
    }
}
