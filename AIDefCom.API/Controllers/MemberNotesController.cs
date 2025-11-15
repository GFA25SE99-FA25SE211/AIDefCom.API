using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.MemberNote;
using AIDefCom.Service.Services.MemberNoteService;
using Microsoft.AspNetCore.Mvc;

namespace AIDefCom.API.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Retrieving all member notes");
            var data = await _service.GetAllAsync();
            
            return Ok(new ApiResponse<IEnumerable<MemberNoteReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Member notes"),
                Data = data
            });
        }

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
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Member note"),
                Data = note
            });
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroupId(string groupId)
        {
            _logger.LogInformation("Retrieving member notes for group ID: {GroupId}", groupId);
            var data = await _service.GetByGroupIdAsync(groupId);
            
            return Ok(new ApiResponse<IEnumerable<MemberNoteReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Member notes by group"),
                Data = data
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            _logger.LogInformation("Retrieving member notes for user ID: {UserId}", userId);
            var data = await _service.GetByUserIdAsync(userId);
            
            return Ok(new ApiResponse<IEnumerable<MemberNoteReadDto>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Member notes by user"),
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MemberNoteCreateDto dto)
        {
            _logger.LogInformation("Creating new member note for group {GroupId} by user {UserId}", dto.GroupId, dto.UserId);
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            _logger.LogInformation("Member note created with ID: {Id}", id);
            
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<MemberNoteReadDto>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

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
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Member note")
            });
        }

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
