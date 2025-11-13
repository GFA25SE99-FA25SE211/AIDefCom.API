using AIDefCom.Service.Dto.Lecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.LecturerService
{
    public interface ILecturerService
    {
        Task<IEnumerable<LecturerReadDto>> GetAllAsync();
        Task<LecturerReadDto?> GetByIdAsync(string id);
        Task<IEnumerable<LecturerReadDto>> GetByDepartmentAsync(string department);
        Task<IEnumerable<LecturerReadDto>> GetByAcademicRankAsync(string academicRank);
        Task<string> AddAsync(LecturerCreateDto dto);
        Task<bool> UpdateAsync(string id, LecturerUpdateDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
