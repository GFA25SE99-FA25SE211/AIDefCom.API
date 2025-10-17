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
