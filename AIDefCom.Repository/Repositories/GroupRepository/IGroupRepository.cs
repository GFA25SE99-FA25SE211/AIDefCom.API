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
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group?> GetByIdAsync(string id);
        Task<IEnumerable<Group>> GetBySemesterIdAsync(int semesterId);
        Task AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(string id);
        Task<bool> ExistsByProjectCodeAsync(string code);
        IQueryable<Group> Query();

    }
}
