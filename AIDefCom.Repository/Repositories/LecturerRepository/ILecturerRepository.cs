using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.LecturerRepository
{
    public interface ILecturerRepository
    {
        Task<IEnumerable<Lecturer>> GetAllAsync();
        Task<Lecturer?> GetByIdAsync(string id);
        Task<IEnumerable<Lecturer>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Lecturer>> GetByAcademicRankAsync(string academicRank);
        Task AddAsync(Lecturer lecturer);
        Task UpdateAsync(Lecturer lecturer);
        Task DeleteAsync(string id);
    }
}
