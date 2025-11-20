using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.SemesterRepository
{
    public interface ISemesterRepository
    {
        Task<IEnumerable<Semester>> GetAllAsync(bool includeDeleted = false);
        Task<Semester?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<IEnumerable<Semester>> GetByMajorIdAsync(int majorId);
        Task AddAsync(Semester semester);
        Task UpdateAsync(Semester semester);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int year, int majorId);
    }
}
