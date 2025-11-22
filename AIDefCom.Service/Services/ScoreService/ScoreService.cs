using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Score;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ScoreService
{
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ScoreService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ScoreReadDto>> GetAllAsync()
        {
            var scores = await _uow.Scores.GetAllAsync();
            return scores.Select(s => new ScoreReadDto
            {
                Id = s.Id,
                Value = s.Value,
                RubricId = s.RubricId,
                RubricName = s.Rubric?.RubricName,
                EvaluatorId = s.EvaluatorId,
                EvaluatorName = s.Evaluator?.FullName,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName,
                SessionId = s.SessionId,
                Comment = s.Comment,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<ScoreReadDto?> GetByIdAsync(int id)
        {
            var score = await _uow.Scores.GetByIdAsync(id);
            if (score == null) return null;

            return new ScoreReadDto
            {
                Id = score.Id,
                Value = score.Value,
                RubricId = score.RubricId,
                RubricName = score.Rubric?.RubricName,
                EvaluatorId = score.EvaluatorId,
                EvaluatorName = score.Evaluator?.FullName,
                StudentId = score.StudentId,
                StudentName = score.Student?.FullName,
                SessionId = score.SessionId,
                Comment = score.Comment,
                CreatedAt = score.CreatedAt
            };
        }

        public async Task<IEnumerable<ScoreReadDto>> GetBySessionIdAsync(int sessionId)
        {
            var scores = await _uow.Scores.GetBySessionIdAsync(sessionId);
            return scores.Select(s => new ScoreReadDto
            {
                Id = s.Id,
                Value = s.Value,
                RubricId = s.RubricId,
                RubricName = s.Rubric?.RubricName,
                EvaluatorId = s.EvaluatorId,
                EvaluatorName = s.Evaluator?.FullName,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName,
                SessionId = s.SessionId,
                Comment = s.Comment,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<IEnumerable<ScoreReadDto>> GetByStudentIdAsync(string studentId)
        {
            var scores = await _uow.Scores.GetByStudentIdAsync(studentId);
            return scores.Select(s => new ScoreReadDto
            {
                Id = s.Id,
                Value = s.Value,
                RubricId = s.RubricId,
                RubricName = s.Rubric?.RubricName,
                EvaluatorId = s.EvaluatorId,
                EvaluatorName = s.Evaluator?.FullName,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName,
                SessionId = s.SessionId,
                Comment = s.Comment,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<IEnumerable<ScoreReadDto>> GetByEvaluatorIdAsync(string evaluatorId)
        {
            var scores = await _uow.Scores.GetByEvaluatorIdAsync(evaluatorId);
            return scores.Select(s => new ScoreReadDto
            {
                Id = s.Id,
                Value = s.Value,
                RubricId = s.RubricId,
                RubricName = s.Rubric?.RubricName,
                EvaluatorId = s.EvaluatorId,
                EvaluatorName = s.Evaluator?.FullName,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName,
                SessionId = s.SessionId,
                Comment = s.Comment,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<IEnumerable<ScoreReadDto>> GetByRubricIdAsync(int rubricId)
        {
            var scores = await _uow.Scores.GetByRubricIdAsync(rubricId);
            return scores.Select(s => new ScoreReadDto
            {
                Id = s.Id,
                Value = s.Value,
                RubricId = s.RubricId,
                RubricName = s.Rubric?.RubricName,
                EvaluatorId = s.EvaluatorId,
                EvaluatorName = s.Evaluator?.FullName,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName,
                SessionId = s.SessionId,
                Comment = s.Comment,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<int> AddAsync(ScoreCreateDto dto)
        {
            // Validate foreign keys
            var rubric = await _uow.Rubrics.GetByIdAsync(dto.RubricId);
            if (rubric == null)
                throw new KeyNotFoundException($"Rubric with ID {dto.RubricId} not found");

            var evaluator = await _uow.Lecturers.GetByIdAsync(dto.EvaluatorId);
            if (evaluator == null)
                throw new KeyNotFoundException($"Evaluator (Lecturer) with ID {dto.EvaluatorId} not found");

            var student = await _uow.Students.GetByIdAsync(dto.StudentId);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {dto.StudentId} not found");

            var session = await _uow.DefenseSessions.GetByIdAsync(dto.SessionId);
            if (session == null)
                throw new KeyNotFoundException($"Defense session with ID {dto.SessionId} not found");

            var score = _mapper.Map<Score>(dto);
            score.CreatedAt = DateTime.UtcNow;

            await _uow.Scores.AddAsync(score);
            await _uow.SaveChangesAsync();

            return score.Id;
        }

        public async Task<bool> UpdateAsync(int id, ScoreUpdateDto dto)
        {
            var existing = await _uow.Scores.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Scores.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var score = await _uow.Scores.GetByIdAsync(id);
            if (score == null) return false;

            await _uow.Scores.DeleteAsync(id);
            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
