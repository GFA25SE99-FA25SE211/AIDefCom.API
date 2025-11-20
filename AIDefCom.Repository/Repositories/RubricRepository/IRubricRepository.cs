using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.RubricRepository
{
    public interface IRubricRepository
    {
        Task<IEnumerable<Rubric>> GetAllAsync(bool includeDeleted = false);
        Task<Rubric?> GetByIdAsync(int id, bool includeDeleted = false);
        Task AddAsync(Rubric rubric);
        Task UpdateAsync(Rubric rubric);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
        Task<bool> ExistsByNameAsync(string rubricName);
    }
}
