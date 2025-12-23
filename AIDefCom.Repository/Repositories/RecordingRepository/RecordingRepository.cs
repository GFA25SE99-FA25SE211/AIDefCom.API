using System;
using System.Linq;
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

        public async Task<Recording?> GetByIdWithTranscriptAsync(Guid id)
        {
            return await _set
                .Include(r => r.Transcript)
                .FirstOrDefaultAsync(r => r.Id == id);
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

        public async Task<int?> GetReportIdByRecordingIdAsync(Guid recordingId)
        {
            // Recording -> Transcript -> Session -> Report
            var recording = await _set
                .Include(r => r.Transcript)
                    .ThenInclude(t => t!.Session)
                        .ThenInclude(s => s!.Report)
                .FirstOrDefaultAsync(r => r.Id == recordingId);

            return recording?.Transcript?.Session?.Report?.Id;
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
