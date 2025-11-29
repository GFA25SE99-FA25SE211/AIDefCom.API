using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.ProjectTaskRepository
{
    public class ProjectTaskRepository : IProjectTaskRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ProjectTask> _set;

        public ProjectTaskRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<ProjectTask>();
        }

        public async Task<IEnumerable<ProjectTask>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy).ThenInclude(a => a.Lecturer)
                             .Include(t => t.AssignedTo).ThenInclude(a => a.Lecturer)
                             .Include(t => t.Rubric)
                             .Include(t => t.Session)
                             .OrderByDescending(t => t.Id)
                             .ToListAsync();
        }

        public async Task<ProjectTask?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy).ThenInclude(a => a.Lecturer)
                             .Include(t => t.AssignedTo).ThenInclude(a => a.Lecturer)
                             .Include(t => t.Rubric)
                             .Include(t => t.Session)
                             .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<ProjectTask>> GetByAssignerAsync(string assignedById)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy).ThenInclude(a => a.Lecturer)
                             .Include(t => t.AssignedTo).ThenInclude(a => a.Lecturer)
                             .Include(t => t.Rubric)
                             .Include(t => t.Session)
                             .Where(t => t.AssignedById == assignedById)
                             .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> GetByAssigneeAsync(string assignedToId)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy).ThenInclude(a => a.Lecturer)
                             .Include(t => t.AssignedTo).ThenInclude(a => a.Lecturer)
                             .Include(t => t.Rubric)
                             .Include(t => t.Session)
                             .Where(t => t.AssignedToId == assignedToId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> GetByAssigneeAndSessionAsync(string assignedToId, int sessionId)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy).ThenInclude(a => a.Lecturer)
                             .Include(t => t.AssignedTo).ThenInclude(a => a.Lecturer)
                             .Include(t => t.Rubric)
                             .Include(t => t.Session)
                             .Where(t => t.AssignedToId == assignedToId && t.SessionId == sessionId)
                             .ToListAsync();
        }

        public async Task<bool> ExistsBySessionAndRubricAsync(int sessionId, int rubricId, int? excludeTaskId = null)
        {
            var query = _set.AsNoTracking().Where(t => t.SessionId == sessionId && t.RubricId == rubricId);
            if (excludeTaskId.HasValue)
            {
                query = query.Where(t => t.Id != excludeTaskId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task AddAsync(ProjectTask entity)
        {
            await _set.AddAsync(entity);
        }

        public async Task UpdateAsync(ProjectTask entity)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null) return;

            existing.Title = entity.Title;
            existing.Description = entity.Description;
            existing.AssignedById = entity.AssignedById;
            existing.AssignedToId = entity.AssignedToId;
            existing.RubricId = entity.RubricId;
            existing.SessionId = entity.SessionId;
            existing.Status = entity.Status;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _set.FindAsync(id);
            if (existing != null)
                _set.Remove(existing);
        }
    }
}
