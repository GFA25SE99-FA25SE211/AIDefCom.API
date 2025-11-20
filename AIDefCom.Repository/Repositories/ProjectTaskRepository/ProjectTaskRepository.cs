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
                             .Include(t => t.AssignedBy)
                             .Include(t => t.AssignedTo)
                             .Include(t => t.Rubric)
                             .OrderByDescending(t => t.Id)
                             .ToListAsync();
        }

        public async Task<ProjectTask?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy)
                             .Include(t => t.AssignedTo)
                             .Include(t => t.Rubric)
                             .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<ProjectTask>> GetByAssignerAsync(string assignedById)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedTo)
                             .Include(t => t.Rubric)
                             .Where(t => t.AssignedById == assignedById)
                             .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> GetByAssigneeAsync(string assignedToId)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.AssignedBy)
                             .Include(t => t.Rubric)
                             .Where(t => t.AssignedToId == assignedToId)
                             .ToListAsync();
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
