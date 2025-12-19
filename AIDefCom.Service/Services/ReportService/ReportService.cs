using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Report;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ReportService
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ReportService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReportReadDto>> GetAllAsync()
        {
            var list = await _uow.Reports.GetAllAsync();
            return _mapper.Map<IEnumerable<ReportReadDto>>(list);
        }

        public async Task<ReportReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.Reports.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ReportReadDto>(entity);
        }

        public async Task<IEnumerable<ReportReadDto>> GetBySessionIdAsync(int sessionId)
        {
            var list = await _uow.Reports.GetBySessionIdAsync(sessionId);
            return _mapper.Map<IEnumerable<ReportReadDto>>(list);
        }

        public async Task<IEnumerable<ReportReadDto>> GetByLecturerIdAsync(string lecturerId)
        {
            var list = await _uow.Reports.GetByLecturerIdAsync(lecturerId);
            return _mapper.Map<IEnumerable<ReportReadDto>>(list);
        }

        public async Task<int> AddAsync(ReportCreateDto dto)
        {
            // ✅ VALIDATE 1: Kiểm tra DefenseSession tồn tại
            var session = await _uow.DefenseSessions.GetByIdAsync(dto.SessionId);
            if (session == null)
                throw new KeyNotFoundException($"Defense session with ID {dto.SessionId} not found");

            // ✅ VALIDATE 2: Kiểm tra DefenseSession đã hoàn thành chưa
            if (session.Status != "Completed")
                throw new InvalidOperationException($"Cannot create report for defense session with status '{session.Status}'. Only 'Completed' defense sessions can have reports.");

            // ✅ VALIDATE 3: Kiểm tra đã tồn tại Report cho session này chưa (vì quan hệ 1-1)
            var existingReports = await _uow.Reports.GetBySessionIdAsync(dto.SessionId);
            if (existingReports.Any())
                throw new InvalidOperationException($"Defense session {dto.SessionId} already has a report. Each defense session can only have one report (1-1 relationship).");

            // ✅ VALIDATE 4: Kiểm tra FilePath hợp lệ (nếu được cung cấp)
            if (!string.IsNullOrWhiteSpace(dto.FilePath))
            {
                dto.FilePath = dto.FilePath.Trim();
                if (dto.FilePath.Length < 5)
                    throw new ArgumentException("File path must be at least 5 characters long");
            }

            // ✅ VALIDATE 5: Kiểm tra Status hợp lệ
            var validStatuses = new[] { "Pending", "Draft", "Approved", "Rejected" };
            if (!string.IsNullOrWhiteSpace(dto.Status) && !validStatuses.Contains(dto.Status))
                throw new ArgumentException($"Invalid status '{dto.Status}'. Valid statuses are: {string.Join(", ", validStatuses)}");

            var entity = _mapper.Map<Report>(dto);
            entity.GeneratedDate = DateTime.UtcNow;
            entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status; // Set default status to Pending if not provided
            
            await _uow.Reports.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, ReportUpdateDto dto)
        {
            var existing = await _uow.Reports.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            existing.GeneratedDate = DateTime.UtcNow;

            await _uow.Reports.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.Reports.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.Reports.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var entity = await _uow.Reports.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "Approved";
            await _uow.Reports.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id)
        {
            var entity = await _uow.Reports.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = "Rejected";
            await _uow.Reports.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveReportFilePathAsync(int reportId, string filePath)
        {
            var entity = await _uow.Reports.GetByIdAsync(reportId);
            if (entity == null) return false;

            entity.FilePath = filePath;
            await _uow.Reports.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetReportFilePathByIdAsync(int reportId)
        {
            var entity = await _uow.Reports.GetByIdAsync(reportId);
            return entity?.FilePath;
        }

        public async Task<ReportFilePathDto?> GetReportFilePathWithSessionAsync(int reportId)
        {
            var entity = await _uow.Reports.GetByIdAsync(reportId);
            if (entity == null || entity.Session == null) return null;

            return new ReportFilePathDto
            {
                ReportId = entity.Id,
                FilePath = entity.FilePath,
                SessionId = entity.SessionId,
                SessionLocation = entity.Session.Location,
                DefenseDate = entity.Session.DefenseDate,
                StartTime = entity.Session.StartTime,
                EndTime = entity.Session.EndTime,
                SessionStatus = entity.Session.Status,
                GroupId = entity.Session.GroupId,
                CouncilId = entity.Session.CouncilId
            };
        }
    }
}
