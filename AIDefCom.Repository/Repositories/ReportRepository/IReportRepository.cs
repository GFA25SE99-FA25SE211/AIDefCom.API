using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.ReportRepository
{
    public interface IReportRepository
    {
        Task<IEnumerable<Report>> GetAllAsync();
        Task<Report?> GetByIdAsync(int id);
        Task<IEnumerable<Report>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<Report>> GetByLecturerIdAsync(string lecturerId);
        Task AddAsync(Report report);
            Task UpdateAsync(Report report);
            Task DeleteAsync(int id);
            Task HardDeleteBySessionIdsAsync(IEnumerable<int> sessionIds);
        }
}
