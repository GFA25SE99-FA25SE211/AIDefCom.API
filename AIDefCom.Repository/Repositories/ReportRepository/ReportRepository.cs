using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.ReportRepository
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Report> _set;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Report>();
        }

        public async Task<IEnumerable<Report>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(r => r.Session)
                             .OrderByDescending(r => r.GeneratedDate)
                             .ToListAsync();
        }

        public async Task<Report?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(r => r.Session)
                             .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Report>> GetBySessionIdAsync(int sessionId)
        {
            return await _set.AsNoTracking()
                             .Where(r => r.SessionId == sessionId)
                             .ToListAsync();
        }

        public async Task AddAsync(Report report)
        {
            await _set.AddAsync(report);
        }

        public async Task UpdateAsync(Report report)
        {
            var existing = await _set.FirstOrDefaultAsync(r => r.Id == report.Id);
            if (existing == null) return;

            existing.SessionId = report.SessionId;
            existing.FilePath = report.FilePath;
            existing.GeneratedDate = report.GeneratedDate;
            existing.SummaryText = report.SummaryText;
            existing.Status = report.Status;
        }

        public async Task DeleteAsync(int id)
        {
            var report = await _set.FindAsync(id);
            if (report != null)
                _set.Remove(report);
        }
    }
}
