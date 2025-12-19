using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Added for UserManager

namespace AIDefCom.Service.Services.DefenseSessionService
{
    public class DefenseSessionService : IDefenseSessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager; // Inject identity user manager

        public DefenseSessionService(IUnitOfWork uow, IMapper mapper, UserManager<AppUser> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            ExcelHelper.ConfigureExcelPackage();
        }

        // ------------------ CRUD ------------------

        public async Task<IEnumerable<DefenseSessionReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.DefenseSessions.GetAllAsync(includeDeleted);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<DefenseSessionReadDto?> GetByIdAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(id));

            var entity = await _uow.DefenseSessions.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<DefenseSessionReadDto>(entity);
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetByGroupIdAsync(string groupId)
        {
            // Validate GroupId
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("Group ID cannot be null or empty", nameof(groupId));

            var list = await _uow.DefenseSessions.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetByLecturerIdAsync(string lecturerId)
        {
            // Validate LecturerId
            if (string.IsNullOrWhiteSpace(lecturerId))
                throw new ArgumentException("Lecturer ID cannot be null or empty", nameof(lecturerId));

            var list = await _uow.DefenseSessions.GetByLecturerIdAsync(lecturerId);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetByStudentIdAsync(string studentId)
        {
            // Validate StudentId
            if (string.IsNullOrWhiteSpace(studentId))
                throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

            var list = await _uow.DefenseSessions.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<string?> GetLecturerRoleInDefenseSessionAsync(string lecturerId, int defenseSessionId)
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(lecturerId))
                throw new ArgumentException("Lecturer ID cannot be null or empty", nameof(lecturerId));
            
            if (defenseSessionId <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(defenseSessionId));

            return await _uow.DefenseSessions.GetLecturerRoleInDefenseSessionAsync(lecturerId, defenseSessionId);
        }

        public async Task<int> AddAsync(DefenseSessionCreateDto dto)
        {
            // Validate Council exists and is active
            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null)
                throw new KeyNotFoundException($"Council with ID {dto.CouncilId} not found or has been deleted");

            if (!council.IsActive)
                throw new InvalidOperationException($"Council with ID {dto.CouncilId} is not active. Only active councils can conduct defense sessions.");

            // Validate Group exists and not deleted
            var group = await _uow.Groups.GetByIdAsync(dto.GroupId);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {dto.GroupId} not found or has been deleted");

            // Validate Group status
            if (group.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot create defense session for cancelled group '{group.ProjectCode}'");

            // Validate Council and Group belong to same Major
            if (council.MajorId != group.MajorId)
                throw new InvalidOperationException($"Council (Major ID: {council.MajorId}) and Group (Major ID: {group.MajorId}) must belong to the same Major");

            // Validate DefenseDate with Semester
            var semester = await _uow.Semesters.GetByIdAsync(group.SemesterId);
            if (semester == null)
                throw new KeyNotFoundException($"Semester not found for Group {dto.GroupId}");

            if (dto.DefenseDate.Date < semester.StartDate.Date || dto.DefenseDate.Date > semester.EndDate.Date)
                throw new InvalidOperationException($"Defense date must be within semester period ({semester.StartDate:dd/MM/yyyy} - {semester.EndDate:dd/MM/yyyy})");

            // Validate defense date is not in the past (allow today)
            if (dto.DefenseDate.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Defense date cannot be in the past");

            // Validate time range
            if (dto.StartTime >= dto.EndTime)
                throw new InvalidOperationException("Start time must be before end time");

            // Validate minimum session duration (at least 30 minutes)
            var duration = dto.EndTime - dto.StartTime;
            if (duration.TotalMinutes < 30)
                throw new InvalidOperationException("Defense session must be at least 30 minutes long");

            // Validate maximum session duration (no more than 8 hours)
            if (duration.TotalHours > 8)
                throw new InvalidOperationException("Defense session cannot exceed 8 hours");

            // Validate Location
            if (string.IsNullOrWhiteSpace(dto.Location))
                throw new ArgumentException("Location cannot be empty");

            dto.Location = dto.Location.Trim();
            if (dto.Location.Length < 5)
                throw new ArgumentException("Location must be at least 5 characters");

            // Check for duplicate session for the same group
            var existingSessions = await _uow.DefenseSessions.GetByGroupIdAsync(dto.GroupId);
            var hasActiveSession = existingSessions.Any(s => 
                s.Status != "Completed" && 
                s.Status != "Cancelled" && 
                !s.IsDeleted);

            if (hasActiveSession)
                throw new InvalidOperationException($"Group '{group.ProjectCode}' already has an active defense session. Please complete or cancel the existing session first.");

            // Check for council schedule conflicts
            var councilSessions = await _uow.DefenseSessions.Query()
                .Where(s => s.CouncilId == dto.CouncilId && 
                           s.DefenseDate.Date == dto.DefenseDate.Date &&
                           !s.IsDeleted &&
                           s.Status != "Cancelled")
                .ToListAsync();

            foreach (var session in councilSessions)
            {
                // Check if time ranges overlap
                if (TimeRangesOverlap(dto.StartTime, dto.EndTime, session.StartTime, session.EndTime))
                    throw new InvalidOperationException($"Council {dto.CouncilId} already has a defense session scheduled at this time ({session.StartTime:hh\\:mm} - {session.EndTime:hh\\:mm})");
            }

            var entity = _mapper.Map<DefenseSession>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsDeleted = false;
            entity.Status = "Scheduled"; // Auto-set status to Scheduled

            await _uow.DefenseSessions.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, DefenseSessionUpdateDto dto)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(id));

            // Check if DefenseSession exists
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Cannot update completed or cancelled sessions
            if (existing.Status == "Completed")
                throw new InvalidOperationException("Cannot update a completed defense session");

            if (existing.Status == "Cancelled" && dto.Status != "Scheduled")
                throw new InvalidOperationException("Cancelled defense sessions can only be rescheduled (Status = Scheduled)");

            // Validate Council exists and is active
            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null)
                throw new KeyNotFoundException($"Council with ID {dto.CouncilId} not found or has been deleted");

            if (!council.IsActive && dto.Status != "Cancelled")
                throw new InvalidOperationException($"Council with ID {dto.CouncilId} is not active. Cannot assign inactive council to active session.");

            // Validate Group exists and not deleted
            var group = await _uow.Groups.GetByIdAsync(dto.GroupId);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {dto.GroupId} not found or has been deleted");

            // Validate Council and Group belong to same Major
            if (council.MajorId != group.MajorId)
                throw new InvalidOperationException($"Council (Major ID: {council.MajorId}) and Group (Major ID: {group.MajorId}) must belong to the same Major");

            // Validate DefenseDate with Semester
            var semester = await _uow.Semesters.GetByIdAsync(group.SemesterId);
            if (semester == null)
                throw new KeyNotFoundException($"Semester not found for Group {dto.GroupId}");

            if (dto.DefenseDate.Date < semester.StartDate.Date || dto.DefenseDate.Date > semester.EndDate.Date)
                throw new InvalidOperationException($"Defense date must be within semester period ({semester.StartDate:dd/MM/yyyy} - {semester.EndDate:dd/MM/yyyy})");

            // Allow updating past sessions only if changing to Completed or Cancelled
            if (dto.DefenseDate.Date < DateTime.UtcNow.Date && 
                dto.Status != "Completed" && 
                dto.Status != "Cancelled")
                throw new InvalidOperationException("Past defense sessions can only be marked as Completed or Cancelled");

            // Validate time range
            if (dto.StartTime >= dto.EndTime)
                throw new InvalidOperationException("Start time must be before end time");

            // Validate session duration
            var duration = dto.EndTime - dto.StartTime;
            if (duration.TotalMinutes < 30)
                throw new InvalidOperationException("Defense session must be at least 30 minutes long");

            if (duration.TotalHours > 8)
                throw new InvalidOperationException("Defense session cannot exceed 8 hours");

            // Validate Location
            if (string.IsNullOrWhiteSpace(dto.Location))
                throw new ArgumentException("Location cannot be empty");

            dto.Location = dto.Location.Trim();
            if (dto.Location.Length < 5)
                throw new ArgumentException("Location must be at least 5 characters");

            // Validate status transitions
            ValidateStatusTransition(existing.Status, dto.Status);

            // Check for council schedule conflicts (exclude current session)
            if (existing.DefenseDate.Date != dto.DefenseDate.Date || 
                existing.StartTime != dto.StartTime || 
                existing.EndTime != dto.EndTime ||
                existing.CouncilId != dto.CouncilId)
            {
                var councilSessions = await _uow.DefenseSessions.Query()
                    .Where(s => s.Id != id &&
                               s.CouncilId == dto.CouncilId && 
                               s.DefenseDate.Date == dto.DefenseDate.Date &&
                               !s.IsDeleted &&
                               s.Status != "Cancelled")
                    .ToListAsync();

                foreach (var session in councilSessions)
                {
                    if (TimeRangesOverlap(dto.StartTime, dto.EndTime, session.StartTime, session.EndTime))
                        throw new InvalidOperationException($"Council {dto.CouncilId} already has a defense session scheduled at this time ({session.StartTime:hh\\:mm} - {session.EndTime:hh\\:mm})");
                }
            }

            _mapper.Map(dto, existing);
            await _uow.DefenseSessions.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(id));

            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Cannot delete completed sessions
            if (existing.Status == "Completed")
                throw new InvalidOperationException("Cannot delete a completed defense session. Completed sessions must be preserved for record keeping.");

            // Check if session has scores
            var scores = await _uow.Scores.Query()
                .Where(s => s.SessionId == id)
                .ToListAsync();

            if (scores.Any())
                throw new InvalidOperationException($"Cannot delete defense session because it has {scores.Count} score(s) recorded. Please remove scores first.");

            // Check if session has transcripts
            var transcripts = await _uow.Transcripts.GetBySessionIdAsync(id);

            if (transcripts.Any())
                throw new InvalidOperationException($"Cannot delete defense session because it has {transcripts.Count()} transcript(s). Please remove transcripts first.");

            // Check if session has reports
            var reports = await _uow.Reports.GetBySessionIdAsync(id);

            if (reports.Any())
                throw new InvalidOperationException($"Cannot delete defense session because it has {reports.Count()} report(s). Please remove reports first.");

            // Change status to Cancelled before soft delete
            existing.Status = "Cancelled";
            await _uow.DefenseSessions.UpdateAsync(existing);

            await _uow.DefenseSessions.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(id));

            var existing = await _uow.DefenseSessions.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) 
                return false;

            // Check if already active
            if (!existing.IsDeleted)
                throw new InvalidOperationException($"Defense session {id} is already active");

            // Validate that Group still exists
            var group = await _uow.Groups.GetByIdAsync(existing.GroupId);
            if (group == null)
                throw new InvalidOperationException($"Cannot restore defense session because Group with ID {existing.GroupId} no longer exists or has been deleted");

            // Validate that Council still exists
            var council = await _uow.Councils.GetByIdAsync(existing.CouncilId);
            if (council == null)
                throw new InvalidOperationException($"Cannot restore defense session because Council with ID {existing.CouncilId} no longer exists");

            if (!council.IsActive)
                throw new InvalidOperationException($"Cannot restore defense session because Council {existing.CouncilId} is no longer active");

            await _uow.DefenseSessions.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserReadDto>> GetUsersByDefenseSessionIdAsync(int defenseSessionId)
        {
            // Validate ID
            if (defenseSessionId <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(defenseSessionId));

            // 1️⃣ Lấy session
            var session = await _uow.DefenseSessions.GetByIdAsync(defenseSessionId);
            if (session == null)
                return Enumerable.Empty<UserReadDto>();

            var result = new List<UserReadDto>();

            var lecturerAssignments = await _uow.CommitteeAssignments.Query()
                .Include(ca => ca.Lecturer)
                .Include(ca => ca.CouncilRole)
                .Where(ca => ca.CouncilId == session.CouncilId)
                .ToListAsync();

            foreach (var ca in lecturerAssignments)
            {
                if (ca.Lecturer != null)
                {
                    var lecturerRoles = await _userManager.GetRolesAsync(ca.Lecturer);
                    var loginRole = lecturerRoles.FirstOrDefault();
                    result.Add(new UserReadDto
                    {
                        Id = ca.Lecturer.Id,
                        FullName = ca.Lecturer.FullName,
                        Email = ca.Lecturer.Email ?? string.Empty,
                        //Role = loginRole ?? ca.CouncilRole?.RoleName ?? "Committee Member"
                        Role = ca.CouncilRole?.RoleName ?? "Committee Member"
                    });
                }
            }

            var studentGroups = await _uow.StudentGroups.Query()
                .Include(sg => sg.Student)
                .Where(sg => sg.GroupId == session.GroupId)
                .ToListAsync();

            foreach (var sg in studentGroups)
            {
                if (sg.Student != null)
                {
                    var studentRoles = await _userManager.GetRolesAsync(sg.Student);
                    var loginRole = studentRoles.FirstOrDefault(); // fallback
                    result.Add(new UserReadDto
                    {
                        Id = sg.Student.Id,
                        FullName = sg.Student.FullName,
                        Email = sg.Student.Email ?? string.Empty,
                        Role = loginRole
                    });
                }
            }

            return result;
        }

        // Import methods
        public async Task<DefenseSessionImportResultDto> ImportDefenseSessionsAsync(IFormFile file)
        {
            var result = new DefenseSessionImportResultDto();

            try
            {
                // ✅ VALIDATION 1: File null check
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File cannot be null or empty. Please upload a valid Excel file.");

                // ✅ VALIDATION 2: File size check (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    throw new ArgumentException($"File size exceeds the maximum allowed size of {maxFileSize / (1024 * 1024)}MB. Current file size: {file.Length / (1024.0 * 1024):F2}MB");

                // ✅ VALIDATION 3: File extension check
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                    throw new ArgumentException($"Invalid file type. Only Excel files (.xlsx, .xls) are allowed. Current file type: {fileExtension}");

                // ✅ VALIDATION 4: File name security check
                if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
                    throw new ArgumentException("Invalid file name. File name cannot contain special characters (/, \\, ..)");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                
                // ✅ VALIDATION 5: Stream content check
                if (stream.Length == 0)
                    throw new ArgumentException("File content is empty. Unable to read file data.");

                stream.Position = 0;
                
                ExcelPackage package;
                try
                {
                    package = new ExcelPackage(stream);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Unable to read Excel file. Please ensure the file is a valid Excel document. Details: {ex.Message}");
                }

                using (package)
                {
                    // ✅ VALIDATION 6: Worksheet existence check
                    if (package.Workbook.Worksheets.Count == 0)
                        throw new ArgumentException("Excel file contains no worksheets. Please use the standard template.");

                    var worksheet = package.Workbook.Worksheets[0];
                    
                    if (worksheet == null)
                        throw new ArgumentException("Unable to read the first worksheet in the Excel file.");

                    // ✅ VALIDATION 7: Worksheet has data check
                    if (worksheet.Dimension == null || worksheet.Dimension.Rows < 2)
                        throw new ArgumentException("Excel file contains no data. At least 1 header row and 1 data row are required.");

                    // ✅ VALIDATION 8: Headers validation
                    var expectedHeaders = new[] { "Mã đề tài", "Tên đề tài Tiếng Anh/ Tiếng Nhật", "Tên đề tài Tiếng Việt", "GVHD", "Ngày BVKL", "Giờ", "Hội đồng", "Địa điểm", "Nhiệm vụ TVHĐ", "Họ và tên TVHĐ", "Email" };
                    var actualHeaders = new List<string>();
                    
                    for (int col = 1; col <= 11; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Text?.Trim() ?? "";
                        actualHeaders.Add(headerValue);
                    }

                    if (!expectedHeaders.SequenceEqual(actualHeaders))
                    {
                        var missingHeaders = expectedHeaders.Except(actualHeaders).ToList();
                        var extraHeaders = actualHeaders.Except(expectedHeaders).Where(h => !string.IsNullOrEmpty(h)).ToList();
                        
                        var errorMsg = "Invalid Excel template.\n";
                        if (missingHeaders.Any())
                            errorMsg += $"Missing columns: {string.Join(", ", missingHeaders)}\n";
                        if (extraHeaders.Any())
                            errorMsg += $"Unexpected columns: {string.Join(", ", extraHeaders)}\n";
                        errorMsg += $"Expected columns: {string.Join(", ", expectedHeaders)}";
                        
                        throw new ArgumentException(errorMsg);
                    }

                    // ✅ VALIDATION 9: Row count check
                    int rowCount = worksheet.Dimension.Rows;
                    const int maxRows = 1000;
                    if (rowCount > maxRows + 1)
                        throw new ArgumentException($"File exceeds the maximum allowed rows ({maxRows} data rows). Current file has {rowCount - 1} data rows.");

                    result.TotalRows = rowCount - 1;

                    // ✅ VALIDATION 10: Minimum data rows check
                    if (rowCount < 2)
                        throw new ArgumentException("Excel file contains no data rows. At least 1 data row is required after the header.");

                    return await ProcessDefenseSessionImportRows(worksheet, rowCount, result);
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred during import: {ex.Message}", ex);
            }
        }

        private async Task<DefenseSessionImportResultDto> ProcessDefenseSessionImportRows(
            ExcelWorksheet worksheet, int rowCount, DefenseSessionImportResultDto result)
        {
            var createdSessions = new Dictionary<string, int>();
            var createdCouncils = new Dictionary<int, bool>();
            
            // ✅ Track schedule conflicts within the file for each council
            var councilSchedules = new Dictionary<int, List<(DateTime date, TimeSpan startTime, TimeSpan endTime, string projectCode, int row)>>();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var projectCode = worksheet.Cells[row, 1].Text?.Trim();
                    var defenseDate = worksheet.Cells[row, 5].Text?.Trim();
                    var timeRange = worksheet.Cells[row, 6].Text?.Trim();
                    var councilIdStr = worksheet.Cells[row, 7].Text?.Trim();
                    var location = worksheet.Cells[row, 8].Text?.Trim();
                    var memberRole = worksheet.Cells[row, 9].Text?.Trim();
                    var memberName = worksheet.Cells[row, 10].Text?.Trim();
                    var memberEmail = worksheet.Cells[row, 11].Text?.Trim();

                    // ✅ VALIDATION: Required fields
                    var missingFields = new List<string>();
                    if (string.IsNullOrEmpty(projectCode)) missingFields.Add("Mã đề tài");
                    if (string.IsNullOrEmpty(defenseDate)) missingFields.Add("Ngày BVKL");
                    if (string.IsNullOrEmpty(timeRange)) missingFields.Add("Giờ");
                    if (string.IsNullOrEmpty(councilIdStr)) missingFields.Add("Hội đồng");
                    if (string.IsNullOrEmpty(location)) missingFields.Add("Địa điểm");

                    if (missingFields.Any())
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = string.Join(", ", missingFields),
                            ErrorMessage = $"Required field(s) missing: {string.Join(", ", missingFields)}",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // ✅ VALIDATION: Council ID format
                    if (!int.TryParse(councilIdStr, out int councilId))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Hội đồng",
                            ErrorMessage = $"Invalid Council ID format. Must be a number. Current value: '{councilIdStr}'",
                            Value = councilIdStr
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (councilId <= 0)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Hội đồng",
                            ErrorMessage = "Council ID must be greater than 0",
                            Value = councilIdStr
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // ✅ VALIDATION: Check if Council exists (cache result)
                    if (!createdCouncils.ContainsKey(councilId))
                    {
                        var council = await _uow.Councils.GetByIdAsync(councilId);
                        if (council == null)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Hội đồng",
                                ErrorMessage = $"Council with ID {councilId} not found in the system",
                                Value = councilIdStr
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // ✅ VALIDATION: Check if Council is active
                        if (!council.IsActive)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Hội đồng",
                                ErrorMessage = $"Council with ID {councilId} is not active. Only active councils can conduct defense sessions",
                                Value = councilIdStr
                            });
                            result.FailureCount++;
                            continue;
                        }

                        createdCouncils[councilId] = true;
                    }

                    // ✅ VALIDATION: Parse and validate date
                    DateTime parsedDate;
                    var dateFormats = new[] { "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };

                    if (!DateTime.TryParseExact(defenseDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        if (!DateTime.TryParse(defenseDate, out parsedDate))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Ngày BVKL",
                                ErrorMessage = "Invalid date format. Use dd/MM/yyyy (e.g., 10/03/2025) or MM/dd/yyyy (e.g., 03/10/2025)",
                                Value = defenseDate
                            });
                            result.FailureCount++;
                            continue;
                        }
                    }

                    // ✅ VALIDATION: Date must not be in the past
                    if (parsedDate.Date < DateTime.UtcNow.Date)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Ngày BVKL",
                            ErrorMessage = $"Defense date cannot be in the past. Date: {parsedDate:dd/MM/yyyy}",
                            Value = defenseDate
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // ✅ VALIDATION: Parse Time Range
                    TimeSpan startTime, endTime;
                    if (!TryParseTimeRange(timeRange, out startTime, out endTime))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Giờ",
                            ErrorMessage = "Invalid time format. Use format like '17h30-19h00' or '17:30-19:00'",
                            Value = timeRange
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // ✅ VALIDATION: Time range validation
                    if (startTime >= endTime)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Giờ",
                            ErrorMessage = "Start time must be before end time",
                            Value = timeRange
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var duration = endTime - startTime;
                    if (duration.TotalMinutes < 30)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Giờ",
                            ErrorMessage = "Defense session must be at least 30 minutes long",
                            Value = timeRange
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (duration.TotalHours > 8)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Giờ",
                            ErrorMessage = "Defense session cannot exceed 8 hours",
                            Value = timeRange
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // ✅ VALIDATION: Location
                    if (location.Length < 5)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Địa điểm",
                            ErrorMessage = "Location must be at least 5 characters long",
                            Value = location
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (location.Length > 500)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Địa điểm",
                            ErrorMessage = "Location cannot exceed 500 characters",
                            Value = location
                        });
                        result.FailureCount++;
                        continue;
                    }

                    string sessionKey = $"{projectCode}_{defenseDate}_{timeRange}";

                    // Check if defense session already created for this row group
                    int defenseSessionId;
                    if (createdSessions.ContainsKey(sessionKey))
                    {
                        defenseSessionId = createdSessions[sessionKey];
                    }
                    else
                    {
                        // ✅ VALIDATION: Check if Group exists
                        var group = await _uow.Groups.GetByProjectCodeAsync(projectCode);
                        if (group == null)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Mã đề tài",
                                ErrorMessage = $"Group with project code '{projectCode}' not found in the system",
                                Value = projectCode
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // ✅ VALIDATION: Check if group already has active defense session
                        var existingGroupSessions = await _uow.DefenseSessions.GetByGroupIdAsync(group.Id);
                        var hasActiveSession = existingGroupSessions.Any(s => 
                            s.Status != "Completed" && 
                            s.Status != "Cancelled" && 
                            !s.IsDeleted);

                        if (hasActiveSession)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Mã đề tài",
                                ErrorMessage = $"Group '{projectCode}' already has an active defense session. Please complete or cancel the existing session first",
                                Value = projectCode
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // ✅ VALIDATION: Validate DefenseDate with Semester
                        var semester = await _uow.Semesters.GetByIdAsync(group.SemesterId);
                        if (semester == null)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Ngày BVKL",
                                ErrorMessage = $"Semester not found for Group '{projectCode}'",
                                Value = defenseDate
                            });
                            result.FailureCount++;
                            continue;
                        }

                        if (parsedDate.Date < semester.StartDate.Date || parsedDate.Date > semester.EndDate.Date)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Ngày BVKL",
                                ErrorMessage = $"Defense date must be within semester period ({semester.StartDate:dd/MM/yyyy} - {semester.EndDate:dd/MM/yyyy})",
                                Value = defenseDate
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // ✅ VALIDATION: Check Council schedule conflicts in DATABASE
                        var existingCouncilSessions = await _uow.DefenseSessions.Query()
                            .Where(s => s.CouncilId == councilId && 
                                       s.DefenseDate.Date == parsedDate.Date &&
                                       !s.IsDeleted &&
                                       s.Status != "Cancelled")
                            .ToListAsync();

                        bool hasConflictInDb = false;
                        foreach (var existingSession in existingCouncilSessions)
                        {
                            if (TimeRangesOverlap(startTime, endTime, existingSession.StartTime, existingSession.EndTime))
                            {
                                result.Errors.Add(new ImportErrorDto
                                {
                                    Row = row,
                                    Field = "Giờ, Hội đồng",
                                    ErrorMessage = $"Schedule conflict: Council {councilId} already has a defense session on {parsedDate:dd/MM/yyyy} from {existingSession.StartTime:hh\\:mm} to {existingSession.EndTime:hh\\:mm}",
                                    Value = $"{timeRange}, {councilIdStr}"
                                });
                                result.FailureCount++;
                                hasConflictInDb = true;
                                break;
                            }
                        }

                        if (hasConflictInDb)
                            continue;

                        // ✅ VALIDATION: Check Council schedule conflicts within the IMPORT FILE
                        if (!councilSchedules.ContainsKey(councilId))
                        {
                            councilSchedules[councilId] = new List<(DateTime, TimeSpan, TimeSpan, string, int)>();
                        }

                        var fileSchedules = councilSchedules[councilId].Where(s => s.date.Date == parsedDate.Date).ToList();
                        bool hasConflictInFile = false;
                        foreach (var fileSchedule in fileSchedules)
                        {
                            if (TimeRangesOverlap(startTime, endTime, fileSchedule.startTime, fileSchedule.endTime))
                            {
                                result.Errors.Add(new ImportErrorDto
                                {
                                    Row = row,
                                    Field = "Giờ, Hội đồng",
                                    ErrorMessage = $"Schedule conflict within file: Council {councilId} is already scheduled on {parsedDate:dd/MM/yyyy} from {fileSchedule.startTime:hh\\:mm} to {fileSchedule.endTime:hh\\:mm} for project '{fileSchedule.projectCode}' (row {fileSchedule.row})",
                                    Value = $"{timeRange}, {councilIdStr}"
                                });
                                result.FailureCount++;
                                hasConflictInFile = true;
                                break;
                            }
                        }

                        if (hasConflictInFile)
                            continue;

                        // Add to schedule tracker
                        councilSchedules[councilId].Add((parsedDate, startTime, endTime, projectCode, row));

                        // Create DefenseSession
                        var defenseSession = new DefenseSession
                        {
                            GroupId = group.Id,
                            Location = location,
                            DefenseDate = parsedDate,
                            StartTime = startTime,
                            EndTime = endTime,
                            Status = "Scheduled",
                            CouncilId = councilId,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };

                        await _uow.DefenseSessions.AddAsync(defenseSession);
                        await _uow.SaveChangesAsync();

                        defenseSessionId = defenseSession.Id;
                        createdSessions[sessionKey] = defenseSessionId;
                        result.CreatedDefenseSessionIds.Add(defenseSessionId);
                    }

                    // Process committee member if provided
                    if (!string.IsNullOrEmpty(memberEmail) && !string.IsNullOrEmpty(memberRole))
                    {
                        // ✅ VALIDATION: Email format for member
                        if (!memberEmail.Contains("@") || !memberEmail.Contains("."))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Email",
                                ErrorMessage = $"Invalid email format for committee member: '{memberEmail}'",
                                Value = memberEmail
                            });
                        }
                        else
                        {
                            var lecturer = await _uow.Lecturers.GetByEmailAsync(memberEmail);
                            if (lecturer == null)
                            {
                                result.Errors.Add(new ImportErrorDto
                                {
                                    Row = row,
                                    Field = "Email",
                                    ErrorMessage = $"Lecturer not found with email: {memberEmail}",
                                    Value = memberEmail
                                });
                            }
                            else
                            {
                                var councilRole = await _uow.CouncilRoles.GetByRoleNameAsync(memberRole);
                                if (councilRole == null)
                                {
                                    result.Errors.Add(new ImportErrorDto
                                    {
                                        Row = row,
                                        Field = "Nhiệm vụ TVHĐ",
                                        ErrorMessage = $"Council role not found: {memberRole}",
                                        Value = memberRole
                                    });
                                }
                                else
                                {
                                    var existingAssignment = await _uow.CommitteeAssignments.Query()
                                        .Where(ca => ca.LecturerId == lecturer.Id && 
                                                    ca.CouncilId == councilId &&
                                                    !ca.IsDeleted)
                                        .ToListAsync();

                                    if (!existingAssignment.Any())
                                    {
                                        var assignment = new CommitteeAssignment
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            LecturerId = lecturer.Id,
                                            CouncilId = councilId,
                                            CouncilRoleId = councilRole.Id,
                                            IsDeleted = false
                                        };

                                        await _uow.CommitteeAssignments.AddAsync(assignment);
                                    }
                                }
                            }
                        }
                    }

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "General",
                        ErrorMessage = $"Unexpected error: {ex.Message}",
                        Value = ""
                    });
                    result.FailureCount++;
                }
            }

            await _uow.SaveChangesAsync();

            result.Message = $"Import completed. {result.CreatedDefenseSessionIds.Count} defense sessions created successfully with committee members. Total rows processed: {result.SuccessCount}, {result.FailureCount} failed.";
            return result;
        }

        public byte[] GenerateDefenseSessionTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("DefenseSessions");

            // Headers
            worksheet.Cells[1, 1].Value = "Mã đề tài";
            worksheet.Cells[1, 2].Value = "Tên đề tài Tiếng Anh/ Tiếng Nhật";
            worksheet.Cells[1, 3].Value = "Tên đề tài Tiếng Việt";
            worksheet.Cells[1, 4].Value = "GVHD";
            worksheet.Cells[1, 5].Value = "Ngày BVKL";
            worksheet.Cells[1, 6].Value = "Giờ";
            worksheet.Cells[1, 7].Value = "Hội đồng";
            worksheet.Cells[1, 8].Value = "Địa điểm";
            worksheet.Cells[1, 9].Value = "Nhiệm vụ TVHĐ";
            worksheet.Cells[1, 10].Value = "Họ và tên TVHĐ";
            worksheet.Cells[1, 11].Value = "Email";

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
            }

            // Add helpful comments
            worksheet.Cells[1, 1].AddComment("Project code must exist in the system", "System");
            worksheet.Cells[1, 5].AddComment("Format: dd/MM/yyyy (e.g., 10/3/2025)", "System");
            worksheet.Cells[1, 6].AddComment("Format: HHhMM-HHhMM (e.g., 17h30-19h00)", "System");
            worksheet.Cells[1, 7].AddComment("Council ID must exist in the system", "System");

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        private bool TryParseTimeRange(string timeRange, out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;

            if (string.IsNullOrEmpty(timeRange))
                return false;

            // Format: "17h30-19h00" or "17:30-19:00"
            var parts = timeRange.Split('-');
            if (parts.Length != 2)
                return false;

            return TryParseTime(parts[0].Trim(), out startTime) && TryParseTime(parts[1].Trim(), out endTime);
        }

        private bool TryParseTime(string time, out TimeSpan result)
        {
            result = TimeSpan.Zero;

            if (string.IsNullOrEmpty(time))
                return false;

            // Remove 'h' and replace with ':'
            time = time.Replace("h", ":").Replace("H", ":");

            return TimeSpan.TryParse(time, out result);
        }

        /// <summary>
        /// Validates defense session status transitions
        /// </summary>
        private void ValidateStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid transitions
            var validTransitions = new Dictionary<string, string[]>
            {
                ["Scheduled"] = new[] { "InProgress", "Postponed", "Cancelled" },
                ["InProgress"] = new[] { "Completed", "Postponed", "Cancelled" },
                ["Completed"] = new string[] { }, // Cannot change from Completed
                ["Cancelled"] = new[] { "Scheduled" }, // Can only reschedule
                ["Postponed"] = new[] { "Scheduled", "Cancelled" }
            };

            if (currentStatus == newStatus)
                return; // No change

            if (!validTransitions.ContainsKey(currentStatus))
                throw new InvalidOperationException($"Unknown current status: {currentStatus}");

            if (!validTransitions[currentStatus].Contains(newStatus))
                throw new InvalidOperationException($"Invalid status transition from '{currentStatus}' to '{newStatus}'. Allowed transitions: {string.Join(", ", validTransitions[currentStatus])}");
        }

        /// <summary>
        /// Checks if two time ranges overlap
        /// </summary>
        private bool TimeRangesOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && end1 > start2;
        }

        /// <summary>
        /// Change defense session status (only allows Scheduled -> InProgress -> Completed)
        /// </summary>
        public async Task<bool> ChangeStatusAsync(int id, string newStatus)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(id));

            // Validate new status
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("New status cannot be empty", nameof(newStatus));

            var validStatuses = new[] { "Scheduled", "InProgress", "Completed" };
            if (!validStatuses.Contains(newStatus))
                throw new ArgumentException($"Invalid status. Only {string.Join(", ", validStatuses)} are allowed for status change.");

            // Get existing session
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null)
                return false;

            // Validate status transition (only forward progression allowed)
            var allowedTransitions = new Dictionary<string, string[]>
            {
                ["Scheduled"] = new[] { "InProgress" },
                ["InProgress"] = new[] { "Completed" },
                ["Completed"] = new string[] { }, // Cannot change from Completed
                ["Cancelled"] = new string[] { }, // Cannot change from Cancelled
                ["Postponed"] = new string[] { }  // Cannot change from Postponed (use Update API instead)
            };

            if (!allowedTransitions.ContainsKey(existing.Status))
                throw new InvalidOperationException($"Unknown current status: {existing.Status}");

            if (!allowedTransitions[existing.Status].Contains(newStatus))
            {
                var allowed = allowedTransitions[existing.Status].Length > 0 
                    ? string.Join(", ", allowedTransitions[existing.Status]) 
                    : "none";
                throw new InvalidOperationException(
                    $"Invalid status transition from '{existing.Status}' to '{newStatus}'. Allowed transitions: {allowed}");
            }

            // Update status
            existing.Status = newStatus;
            await _uow.DefenseSessions.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Update total score for a defense session
        /// </summary>
        public async Task<bool> UpdateTotalScoreAsync(int id, DefenseSessionTotalScoreUpdateDto dto)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Defense session ID must be greater than 0", nameof(id));

            // Validate score precision (max 2 decimal places)
            if (Math.Round(dto.TotalScore, 2) != dto.TotalScore)
                throw new ArgumentException("Total score must have at most 2 decimal places");

            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Only allow updating score for Completed defense sessions
            if (existing.Status != "Completed")
                throw new InvalidOperationException($"Cannot update total score for defense session with status '{existing.Status}'. Only 'Completed' defense sessions can have their scores updated.");

            existing.TotalScore = dto.TotalScore;
            await _uow.DefenseSessions.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
