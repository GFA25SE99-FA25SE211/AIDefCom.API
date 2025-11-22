using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.GroupRepository
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllAsync(bool includeDeleted = false);
        Task<Group?> GetByIdAsync(string id, bool includeDeleted = false);
        Task<Group?> GetByProjectCodeAsync(string projectCode, bool includeDeleted = false);
        Task<IEnumerable<Group>> GetBySemesterIdAsync(int semesterId, bool includeDeleted = false);
        Task AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(string id);
        Task SoftDeleteAsync(string id);
        Task RestoreAsync(string id);
        Task<bool> ExistsByProjectCodeAsync(string code);
        IQueryable<Group> Query();
    }
}
