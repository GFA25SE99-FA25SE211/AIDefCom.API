using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.CouncilRepository
{
    public interface ICouncilRepository
    {
        Task<IEnumerable<Council>> GetAllAsync(bool includeInactive = false);
        Task<Council?> GetByIdAsync(int id);
        Task AddAsync(Council entity);
        Task UpdateAsync(Council entity);
        Task SoftDeleteAsync(int id);   
        Task RestoreAsync(int id);      
    }
}
