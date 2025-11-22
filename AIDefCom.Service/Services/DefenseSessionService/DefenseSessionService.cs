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

namespace AIDefCom.Service.Services.DefenseSessionService
{
    public class DefenseSessionService : IDefenseSessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DefenseSessionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
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
            var entity = await _uow.DefenseSessions.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<DefenseSessionReadDto>(entity);
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetByGroupIdAsync(string groupId)
        {
            var list = await _uow.DefenseSessions.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<int> AddAsync(DefenseSessionCreateDto dto)
        {
            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null)
                throw new ArgumentException($"Council with id {dto.CouncilId} not found.");

            var group = await _uow.Groups.GetByIdAsync(dto.GroupId);
            if (group == null)
                throw new ArgumentException($"Group with id {dto.GroupId} not found.");

            var entity = _mapper.Map<DefenseSession>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.DefenseSessions.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, DefenseSessionUpdateDto dto)
        {
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) return false;

            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null) return false;

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
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.DefenseSessions.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var existing = await _uow.DefenseSessions.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) return false;

            await _uow.DefenseSessions.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserReadDto>> GetUsersByDefenseSessionIdAsync(int defenseSessionId)
        {
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
                    result.Add(new UserReadDto
                    {
                        Id = ca.Lecturer.Id,
                        FullName = ca.Lecturer.FullName,
                        Email = ca.Lecturer.Email ?? string.Empty,
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
                    result.Add(new UserReadDto
                    {
                        Id = sg.Student.Id,
                        FullName = sg.Student.FullName,
                        Email = sg.Student.Email ?? string.Empty,
                        Role = sg.GroupRole ?? "Student"
                    });
                }
            }

            return result;
        }

        // Import methods
        public async Task<DefenseSessionImportResultDto> ImportDefenseSessionsAsync(IFormFile file)
        {
            var result = new DefenseSessionImportResultDto();

            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                throw new ArgumentException("File must be an Excel file (.xlsx or .xls)");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet == null)
                throw new ArgumentException("Excel file has no worksheets");

            // Expected headers based on Excel template
            var expectedHeaders = new[] { "Mã đề tài", "Tên đề tài Tiếng Anh/ Tiếng Nhật", "Tên đề tài Tiếng Việt", "GVHD", "Ngày BVKL", "Giờ", "Hội đồng", "Địa điểm", "Nhiệm vụ TVHĐ", "Họ và tên TVHĐ", "Email" };
            var actualHeaders = new List<string>();
            
            for (int col = 1; col <= 11; col++)
                actualHeaders.Add(worksheet.Cells[1, col].Text?.Trim() ?? "");

            if (!expectedHeaders.SequenceEqual(actualHeaders))
                throw new ArgumentException($"Invalid Excel template. Expected headers: {string.Join(", ", expectedHeaders)}");

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            result.TotalRows = rowCount - 1;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var projectCode = worksheet.Cells[row, 1].Text?.Trim(); // Mã đề tài
                    var defenseDate = worksheet.Cells[row, 5].Text?.Trim(); // Ngày BVKL
                    var timeRange = worksheet.Cells[row, 6].Text?.Trim(); // Giờ
                    var councilIdStr = worksheet.Cells[row, 7].Text?.Trim(); // Hội đồng
                    var location = worksheet.Cells[row, 8].Text?.Trim(); // Địa điểm

                    // Validate required fields
                    if (string.IsNullOrEmpty(projectCode) || string.IsNullOrEmpty(defenseDate) || 
                        string.IsNullOrEmpty(timeRange) || string.IsNullOrEmpty(councilIdStr) || 
                        string.IsNullOrEmpty(location))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Required",
                            ErrorMessage = "Project Code, Defense Date, Time, Council, and Location are required",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Get Group by ProjectCode
                    var group = await _uow.Groups.GetByProjectCodeAsync(projectCode);
                    if (group == null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "ProjectCode",
                            ErrorMessage = "DEF404: Group not found with this project code",
                            Value = projectCode
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Parse Council ID
                    if (!int.TryParse(councilIdStr, out int councilId))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "CouncilId",
                            ErrorMessage = "Invalid Council ID format",
                            Value = councilIdStr
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check if Council exists
                    var council = await _uow.Councils.GetByIdAsync(councilId);
                    if (council == null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "CouncilId",
                            ErrorMessage = "DEF404: Council not found",
                            Value = councilIdStr
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Parse Defense Date
                    DateTime parsedDate;
                    var dateFormats = new[]
                    {
                        "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "M/d/yyyy",
                        "yyyy-MM-dd", "dd-MM-yyyy"
                    };

                    if (!DateTime.TryParseExact(defenseDate, dateFormats, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        if (!DateTime.TryParse(defenseDate, out parsedDate))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "DefenseDate",
                                ErrorMessage = "Invalid date format. Use dd/MM/yyyy (e.g., 10/3/2025)",
                                Value = defenseDate
                            });
                            result.FailureCount++;
                            continue;
                        }
                    }

                    // Parse Time Range (e.g., "17h30-19h00")
                    TimeSpan startTime, endTime;
                    if (!TryParseTimeRange(timeRange, out startTime, out endTime))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Time",
                            ErrorMessage = "Invalid time format. Use format like '17h30-19h00'",
                            Value = timeRange
                        });
                        result.FailureCount++;
                        continue;
                    }

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
                    result.SuccessCount++;
                    result.CreatedDefenseSessionIds.Add(defenseSession.Id);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "General",
                        ErrorMessage = ex.Message,
                        Value = ""
                    });
                    result.FailureCount++;
                }
            }

            // Save all changes
            await _uow.SaveChangesAsync();

            result.Message = $"Import completed. {result.SuccessCount} defense sessions created successfully, {result.FailureCount} failed.";
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

            // Sample data
            worksheet.Cells[2, 1].Value = "SU25SE053";
            worksheet.Cells[2, 2].Value = "Child Growth and Vaccination Tracking Platform";
            worksheet.Cells[2, 3].Value = "Nền tảng theo dõi tăng trưởng và tiêm chủng của trẻ em";
            worksheet.Cells[2, 4].Value = "Đỗ Tấn Nhân";
            worksheet.Cells[2, 5].Value = "10/3/2025";
            worksheet.Cells[2, 6].Value = "17h30-19h00";
            worksheet.Cells[2, 7].Value = "101";
            worksheet.Cells[2, 8].Value = "NVH 601-Cơ sở NVH";
            worksheet.Cells[2, 9].Value = "Chủ tịch HĐ";
            worksheet.Cells[2, 10].Value = "Nguyễn Trọng Tài";
            worksheet.Cells[2, 11].Value = "TaiNT51@fe.edu.vn";

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
    }
}
