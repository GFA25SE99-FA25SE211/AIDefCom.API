using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MajorRepository
{
    public interface IMajorRepository
    {
        Task<IEnumerable<Major>> GetAllAsync(bool includeDeleted = false);
        Task<Major?> GetByIdAsync(int id, bool includeDeleted = false);
        Task AddAsync(Major major);
        Task UpdateAsync(Major major);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
        Task<bool> ExistsByNameAsync(string majorName);
    }
}
