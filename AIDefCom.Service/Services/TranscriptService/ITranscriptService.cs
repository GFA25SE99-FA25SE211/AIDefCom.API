using AIDefCom.Service.Dto.Transcript;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.TranscriptService
{
    public interface ITranscriptService
    {
        Task<IEnumerable<TranscriptReadDto>> GetAllAsync();
        Task<TranscriptReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<TranscriptReadDto>> GetBySessionIdAsync(int sessionId);
        Task<int> AddAsync(TranscriptCreateDto dto);
        Task<bool> UpdateAsync(int id, TranscriptUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
