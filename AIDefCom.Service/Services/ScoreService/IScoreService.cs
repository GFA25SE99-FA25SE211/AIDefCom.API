using AIDefCom.Service.Dto.Score;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ScoreService
{
    public interface IScoreService
    {
        Task<IEnumerable<ScoreReadDto>> GetAllAsync();
        Task<ScoreReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ScoreReadDto>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<ScoreReadDto>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<ScoreReadDto>> GetByEvaluatorIdAsync(string evaluatorId);
        Task<IEnumerable<ScoreReadDto>> GetByRubricIdAsync(int rubricId);
        Task<ScoreReadDto> AddAsync(ScoreCreateDto dto);
        Task<ScoreReadDto?> UpdateAsync(int id, ScoreUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
