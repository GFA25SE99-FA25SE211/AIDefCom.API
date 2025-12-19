using System;
using System.Threading.Tasks;
using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIDefCom.Repository.Repositories.RecordingRepository
{
    public class RecordingRepository : IRecordingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Recording> _set;

        public RecordingRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = context.Set<Recording>();
        }

        public async Task<Recording?> GetByIdAsync(Guid id)
        {
            return await _set.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Recording?> GetByReportIdAsync(int reportId)
        {
            // Report (1-1) -> DefenseSession (1-1) -> Transcript (1-1) -> Recording
            // Query through the chain: Report -> Session -> Transcript -> Recording
            var report = await _context.Reports
                .Where(r => r.Id == reportId)
                .Include(r => r.Session)
                    .ThenInclude(s => s!.Transcript)
                .FirstOrDefaultAsync();

            if (report?.Session?.Transcript == null)
                return null;

            // Get Recording by TranscriptId
            var transcriptId = report.Session.Transcript.Id;
            return await _set
                .Include(r => r.Transcript)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TranscriptId == transcriptId);
        }

        public async Task AddAsync(Recording entity)
        {
            await _set.AddAsync(entity);
        }

        public void Update(Recording entity)
        {
            _set.Update(entity);
        }

        public void Delete(Recording entity)
        {
            _set.Remove(entity);
        }
    }
}
