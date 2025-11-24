using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.ScoreRepository
{
    public class ScoreRepository : IScoreRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Score> _set;

        public ScoreRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Score>();
        }

        public async Task<IEnumerable<Score>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Rubric)
                             .Include(s => s.Evaluator)
                             .Include(s => s.Student)
                             .Include(s => s.Session)
                             .OrderByDescending(s => s.CreatedAt)
                             .ToListAsync();
        }

        public async Task<Score?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Rubric)
                             .Include(s => s.Evaluator)
                             .Include(s => s.Student)
                             .Include(s => s.Session)
                             .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Score>> GetBySessionIdAsync(int sessionId)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Rubric)
                             .Include(s => s.Evaluator)
                             .Include(s => s.Student)
                             .Where(s => s.SessionId == sessionId)
                             .OrderBy(s => s.StudentId)
                             .ThenBy(s => s.RubricId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<Score>> GetByStudentIdAsync(string studentId)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Rubric)
                             .Include(s => s.Evaluator)
                             .Include(s => s.Session)
                             .Where(s => s.StudentId == studentId)
                             .OrderByDescending(s => s.CreatedAt)
                             .ToListAsync();
        }

        public async Task<IEnumerable<Score>> GetByEvaluatorIdAsync(string evaluatorId)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Rubric)
                             .Include(s => s.Student)
                             .Include(s => s.Session)
                             .Where(s => s.EvaluatorId == evaluatorId)
                             .OrderByDescending(s => s.CreatedAt)
                             .ToListAsync();
        }

        public async Task<IEnumerable<Score>> GetByRubricIdAsync(int rubricId)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Evaluator)
                             .Include(s => s.Student)
                             .Include(s => s.Session)
                             .Where(s => s.RubricId == rubricId)
                             .OrderByDescending(s => s.CreatedAt)
                             .ToListAsync();
        }

        public async Task AddAsync(Score score)
        {
            score.CreatedAt = DateTime.UtcNow;
            await _set.AddAsync(score);
        }

        public async Task UpdateAsync(Score score)
        {
            var existing = await _set.FirstOrDefaultAsync(s => s.Id == score.Id);
            if (existing == null) return;

            existing.Value = score.Value;
            existing.Comment = score.Comment;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }

        public IQueryable<Score> Query()
        {
            return _set.AsQueryable();
        }
    }
}
