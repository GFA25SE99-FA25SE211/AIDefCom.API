using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MajorRubricRepository
{
    public interface IMajorRubricRepository
    {
        Task<IEnumerable<MajorRubric>> GetAllAsync();
        Task<MajorRubric?> GetByIdAsync(int id);
        Task<IEnumerable<MajorRubric>> GetByMajorIdAsync(int majorId);
        Task<IEnumerable<MajorRubric>> GetByRubricIdAsync(int rubricId);
        Task AddAsync(MajorRubric entity);
        Task UpdateAsync(MajorRubric entity);   
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int majorId, int rubricId);
    }
}
