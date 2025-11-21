using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.StudentGroupRepository
{
    public interface IStudentGroupRepository
    {
        Task<IEnumerable<StudentGroup>> GetAllAsync(bool includeDeleted = false);
        Task<StudentGroup?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<IEnumerable<StudentGroup>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<StudentGroup>> GetByStudentIdAsync(string studentId);
        Task AddAsync(StudentGroup entity);
        Task UpdateAsync(StudentGroup entity);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
        IQueryable<StudentGroup> Query();
    }
}
