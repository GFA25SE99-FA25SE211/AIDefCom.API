using AIDefCom.Service.Dto.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ReportService
{
    public interface IReportService
    {
        Task<IEnumerable<ReportReadDto>> GetAllAsync();
        Task<ReportReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReportReadDto>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<ReportReadDto>> GetByLecturerIdAsync(string lecturerId);
        Task<int> AddAsync(ReportCreateDto dto);
        Task<bool> UpdateAsync(int id, ReportUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ApproveAsync(int id);
        Task<bool> RejectAsync(int id);
    }
}
