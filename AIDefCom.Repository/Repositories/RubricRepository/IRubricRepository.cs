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
        Task<IEnumerable<Rubric>> GetAllAsync();
        Task<Rubric?> GetByIdAsync(int id);
        Task AddAsync(Rubric rubric);
        Task UpdateAsync(Rubric rubric);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string rubricName);
    }
}
