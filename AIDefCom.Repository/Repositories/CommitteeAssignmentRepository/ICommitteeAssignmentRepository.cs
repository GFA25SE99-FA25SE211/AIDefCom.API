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
        Task<IEnumerable<CommitteeAssignment>> GetAllAsync(bool includeDeleted = false);
        Task<CommitteeAssignment?> GetByIdAsync(string id, bool includeDeleted = false);
        Task<IEnumerable<CommitteeAssignment>> GetByCouncilIdAsync(int councilId);
        Task<IEnumerable<CommitteeAssignment>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<CommitteeAssignment>> GetByLecturerIdAsync(string lecturerId);
        Task<CommitteeAssignment?> GetByLecturerIdAndSessionIdAsync(string lecturerId, int sessionId);
        Task AddAsync(CommitteeAssignment entity);
        Task UpdateAsync(CommitteeAssignment entity);
        Task DeleteAsync(string id);
        Task SoftDeleteAsync(string id);
        Task RestoreAsync(string id);
        IQueryable<CommitteeAssignment> Query();
    }
}
