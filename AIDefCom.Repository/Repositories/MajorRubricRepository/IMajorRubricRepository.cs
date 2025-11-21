using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MajorRubricRepository
{
    public interface IMajorRubricRepository
    {
        Task<IEnumerable<MajorRubric>> GetAllAsync(bool includeDeleted = false);
        Task<MajorRubric?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<IEnumerable<MajorRubric>> GetByMajorIdAsync(int majorId);
        Task<IEnumerable<MajorRubric>> GetByRubricIdAsync(int rubricId);
        Task<bool> ExistsAsync(int majorId, int rubricId);
        Task<IEnumerable<Rubric>> GetRubricsByMajorIdAsync(int majorId);
        Task AddAsync(MajorRubric entity);
        Task UpdateAsync(MajorRubric entity);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
