using AIDefCom.Service.Dto.Note;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.NoteService
{
    public interface INoteService
    {
        Task<IEnumerable<NoteReadDto>> GetAllAsync();
        Task<NoteReadDto?> GetByIdAsync(int id);
        Task<NoteReadDto?> GetBySessionIdAsync(int sessionId);
        Task<int> AddAsync(NoteCreateDto dto);
        Task<bool> UpdateAsync(int id, NoteUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}