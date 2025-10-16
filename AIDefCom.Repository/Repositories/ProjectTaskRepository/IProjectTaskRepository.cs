using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.ProjectTaskRepository
{
    public interface IProjectTaskRepository
    {
        Task<IEnumerable<ProjectTask>> GetAllAsync();
        Task<ProjectTask?> GetByIdAsync(int id);
        Task<IEnumerable<ProjectTask>> GetByAssignerAsync(string assignedById);
        Task<IEnumerable<ProjectTask>> GetByAssigneeAsync(string assignedToId);
        Task AddAsync(ProjectTask entity);
        Task UpdateAsync(ProjectTask entity);
        Task DeleteAsync(int id);
    }
}
