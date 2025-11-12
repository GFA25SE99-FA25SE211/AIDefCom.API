using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.MemberNote;
using AIDefCom.Service.Services.MemberNoteService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for managing member notes
    /// </summary>
    [Route("api/member-notes")]
    [ApiController]
    public class MemberNotesController : ControllerBase
    {
        private readonly IMemberNoteService _service;
        private readonly ILogger<MemberNotesController> _logger;

        public MemberNotesController(IMemberNoteService service, ILogger<MemberNotesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all member notes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all member notes");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<MemberNoteReadDto>>
            {
                MessageCode = MessageCodes.MemberNote_Success0001,
                Message = SystemMessages.MemberNote_Success0001,
                Data = data
            });
        }

        /// <summary>
        /// Get member note by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving member note with ID: {Id}", id);
            var note = await _service.GetByIdAsync(id);
            
            if (note == null)
            {
                _logger.LogWarning("Member note with ID {Id} not found", id);
                throw new KeyNotFoundException($"Member note with ID {id} not found");
            }

            return Ok(new ApiResponse<MemberNoteReadDto>
            {
                MessageCode = MessageCodes.MemberNote_Success0002,
                Message = SystemMessages.MemberNote_Success0002,
                Data = note
            });
        }

        /// <summary>
        /// Get member notes by group ID
        /// </summary>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            _logger.LogInformation("Retrieving member notes for group ID: {GroupId}", groupId);
            var data = await _service.GetByGroupIdAsync(groupId);
            
            return Ok(new ApiResponse<IEnumerable<MemberNoteReadDto>>
            {
                MessageCode = MessageCodes.MemberNote_Success0006,
                Message = SystemMessages.MemberNote_Success0006,
                Data = data
            });
        }

        /// <summary>
        /// Get member notes by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            _logger.LogInformation("Retrieving member notes for user ID: {UserId}", userId);
            var data = await _service.GetByUserIdAsync(userId);
            
            return Ok(new ApiResponse<IEnumerable<MemberNoteReadDto>>
            {
                MessageCode = MessageCodes.MemberNote_Success0007,
                Message = SystemMessages.MemberNote_Success0007,
                Data = data
            });
        }

        /// <summary>
        /// Create a new member note
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MemberNoteCreateDto dto)
        {
            _logger.LogInformation("Creating new member note for group {GroupId} by user {UserId}", dto.GroupId, dto.UserId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Member note created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<MemberNoteReadDto>
            {
                MessageCode = MessageCodes.MemberNote_Success0003,
                Message = SystemMessages.MemberNote_Success0003,
                Data = created
            });
        }

        /// <summary>
        /// Update an existing member note
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MemberNoteUpdateDto dto)
        {
            _logger.LogInformation("Updating member note with ID: {Id}", id);
            var ok = await _service.UpdateAsync(id, dto);
            
            if (!ok)
            {
                _logger.LogWarning("Member note with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"Member note with ID {id} not found");
            }

            _logger.LogInformation("Member note {Id} updated successfully", id);
            return Ok(new ApiResponse<object>
            {
                MessageCode = MessageCodes.MemberNote_Success0004,
                Message = SystemMessages.MemberNote_Success0004
            });
        }

        /// <summary>
        /// Delete a member note
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting member note with ID: {Id}", id);
            var ok = await _service.DeleteAsync(id);
            
            if (!ok)
            {
                _logger.LogWarning("Member note with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException($"Member note with ID {id} not found");
            }

            _logger.LogInformation("Member note {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
