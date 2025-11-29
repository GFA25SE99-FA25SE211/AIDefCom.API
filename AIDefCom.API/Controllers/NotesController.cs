using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Dto.Note;
using AIDefCom.Service.Services.NoteService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AIDefCom.API.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _service;
        private readonly ILogger<NotesController> _logger;

        public NotesController(INoteService service, ILogger<NotesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Notes"),
                Data = data
            });
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            var note = await _service.GetBySessionIdAsync(sessionId);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Note"),
                Data = note
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var note = await _service.GetByIdAsync(id);
            if (note == null) throw new KeyNotFoundException($"Note with ID {id} not found");
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "Note"),
                Data = note
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NoteCreateDto dto)
        {
            var id = await _service.AddAsync(dto);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = created
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NoteUpdateDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) throw new KeyNotFoundException($"Note with ID {id} not found");
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Note")
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) throw new KeyNotFoundException($"Note with ID {id} not found");
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.Deleted, "Note")
            });
        }
    }
}
