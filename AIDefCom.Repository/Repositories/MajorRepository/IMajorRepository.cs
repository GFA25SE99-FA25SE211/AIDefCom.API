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
        Task<IEnumerable<Major>> GetAllAsync();
        Task<Major?> GetByIdAsync(int id);
        Task AddAsync(Major major);
        Task UpdateAsync(Major major);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string majorName);
    }
}
