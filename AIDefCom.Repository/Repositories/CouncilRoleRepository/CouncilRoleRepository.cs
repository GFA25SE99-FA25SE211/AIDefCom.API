using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.CouncilRoleRepository
{
    public class CouncilRoleRepository : ICouncilRoleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<CouncilRole> _set;

        public CouncilRoleRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<CouncilRole>();
        }

        public async Task<IEnumerable<CouncilRole>> GetAllAsync(bool includeDeleted = false)
        {
            IQueryable<CouncilRole> query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.OrderBy(x => x.RoleName).ToListAsync();
        }

        public async Task<CouncilRole?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            IQueryable<CouncilRole> query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CouncilRole?> GetByRoleNameAsync(string roleName, bool includeDeleted = false)
        {
            IQueryable<CouncilRole> query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(x => x.RoleName == roleName);
        }

        public async Task AddAsync(CouncilRole entity)
        {
            await _set.AddAsync(entity);
        }

        public async Task UpdateAsync(CouncilRole entity)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null) return;

            existing.RoleName = entity.RoleName;
            existing.Description = entity.Description;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _set.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
            }
        }

        public async Task RestoreAsync(int id)
        {
            var entity = await _set.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = false;
            }
        }
    }
}
