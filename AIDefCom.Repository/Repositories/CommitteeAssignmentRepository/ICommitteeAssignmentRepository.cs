using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.CommitteeAssignmentRepository
{
    public interface ICommitteeAssignmentRepository
    {
        Task<IEnumerable<CommitteeAssignment>> GetAllAsync();
        Task<CommitteeAssignment?> GetByIdAsync(int id);
        Task<IEnumerable<CommitteeAssignment>> GetByCouncilIdAsync(int councilId);
        Task<IEnumerable<CommitteeAssignment>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<CommitteeAssignment>> GetByLecturerIdAsync(string lecturerId);
        Task AddAsync(CommitteeAssignment entity);
        Task UpdateAsync(CommitteeAssignment entity);
        Task DeleteAsync(int id);
        IQueryable<CommitteeAssignment> Query();

    }
}
