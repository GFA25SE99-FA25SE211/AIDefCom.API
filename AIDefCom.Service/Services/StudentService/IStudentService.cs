using AIDefCom.Service.Dto.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.StudentService
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentReadDto>> GetAllAsync();
        Task<StudentReadDto?> GetByIdAsync(string id);
        Task<IEnumerable<StudentReadDto>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<StudentReadDto>> GetByUserIdAsync(string userId);
        Task<string> AddAsync(StudentCreateDto dto);
        Task<bool> UpdateAsync(string id, StudentUpdateDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
