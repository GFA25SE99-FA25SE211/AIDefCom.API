using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Score;
using AIDefCom.Service.Services.ScoreNotification;
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
        private readonly IScoreNotificationService _notificationService;

        public ScoreService(IUnitOfWork uow, IMapper mapper, IScoreNotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _notificationService = notificationService;
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

        public async Task<ScoreReadDto> AddAsync(ScoreCreateDto dto)
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

            var scoreReadDto = new ScoreReadDto
            {
                Id = score.Id,
                Value = score.Value,
                RubricId = score.RubricId,
                RubricName = rubric.RubricName,
                EvaluatorId = score.EvaluatorId,
                EvaluatorName = evaluator.FullName,
                StudentId = score.StudentId,
                StudentName = student.FullName,
                SessionId = score.SessionId,
                Comment = score.Comment,
                CreatedAt = score.CreatedAt
            };

            // Send real-time notification
            await _notificationService.NotifyScoreCreated(scoreReadDto);

            return scoreReadDto;
        }

        public async Task<ScoreReadDto?> UpdateAsync(int id, ScoreUpdateDto dto)
        {
            var existing = await _uow.Scores.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            await _uow.Scores.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            var scoreReadDto = new ScoreReadDto
            {
                Id = existing.Id,
                Value = existing.Value,
                RubricId = existing.RubricId,
                RubricName = existing.Rubric?.RubricName,
                EvaluatorId = existing.EvaluatorId,
                EvaluatorName = existing.Evaluator?.FullName,
                StudentId = existing.StudentId,
                StudentName = existing.Student?.FullName,
                SessionId = existing.SessionId,
                Comment = existing.Comment,
                CreatedAt = existing.CreatedAt
            };

            // Send real-time notification
            await _notificationService.NotifyScoreUpdated(scoreReadDto);

            return scoreReadDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var score = await _uow.Scores.GetByIdAsync(id);
            if (score == null) return false;

            var scoreReadDto = new ScoreReadDto
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

            await _uow.Scores.DeleteAsync(id);
            await _uow.SaveChangesAsync();

            // Send real-time notification
            await _notificationService.NotifyScoreDeleted(scoreReadDto);

            return true;
        }
    }
}
