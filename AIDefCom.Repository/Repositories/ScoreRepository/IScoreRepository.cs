using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.ScoreRepository
{
    public interface IScoreRepository
    {
        Task<IEnumerable<Score>> GetAllAsync();
        Task<Score?> GetByIdAsync(int id);
        Task<IEnumerable<Score>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<Score>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Score>> GetByEvaluatorIdAsync(string evaluatorId);
        Task<IEnumerable<Score>> GetByRubricIdAsync(int rubricId);
        Task AddAsync(Score score);
        Task UpdateAsync(Score score);
        Task DeleteAsync(int id);
        IQueryable<Score> Query();
    }
}
