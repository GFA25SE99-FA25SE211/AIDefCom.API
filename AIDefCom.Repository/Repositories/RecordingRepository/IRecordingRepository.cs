using System;
using System.Threading.Tasks;
using AIDefCom.Repository.Entities;

namespace AIDefCom.Repository.Repositories.RecordingRepository
{
    public interface IRecordingRepository
    {
        Task<Recording?> GetByIdAsync(Guid id);
        Task<Recording?> GetByIdWithTranscriptAsync(Guid id);
        Task<Recording?> GetByReportIdAsync(int reportId);
        Task<int?> GetReportIdByRecordingIdAsync(Guid recordingId);
        Task AddAsync(Recording entity);
        void Update(Recording entity);
        void Delete(Recording entity);
    }
}
