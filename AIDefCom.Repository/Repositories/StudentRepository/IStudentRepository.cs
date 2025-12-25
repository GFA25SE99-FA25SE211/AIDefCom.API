using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.StudentRepository
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(string id);
        Task<IEnumerable<Student>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<Student>> GetByUserIdAsync(string userId);
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(string id);
        IQueryable<Student> Query(); // Add Query method for advanced queries
    }
}
