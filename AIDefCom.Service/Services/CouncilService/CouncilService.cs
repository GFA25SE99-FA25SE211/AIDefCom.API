using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Council;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CouncilService
{
    public class CouncilService : ICouncilService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CouncilService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            ExcelHelper.ConfigureExcelPackage();
        }

        public async Task<IEnumerable<CouncilReadDto>> GetAllAsync(bool includeInactive = false)
        {
            var list = await _uow.Councils.GetAllAsync(includeInactive);
            return _mapper.Map<IEnumerable<CouncilReadDto>>(list);
        }

        public async Task<CouncilReadDto?> GetByIdAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council ID must be greater than 0", nameof(id));

            var entity = await _uow.Councils.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<CouncilReadDto>(entity);
        }

        public async Task<int> AddAsync(CouncilCreateDto dto)
        {
            // Validate Major exists and not deleted
            var major = await _uow.Majors.GetByIdAsync(dto.MajorId);
            if (major == null)
                throw new KeyNotFoundException($"Major with ID {dto.MajorId} not found or has been deleted");

            // Validate description if provided
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                dto.Description = dto.Description.Trim();
                if (dto.Description.Length < 10)
                    throw new ArgumentException("Council description must be at least 10 characters if provided");
            }

            var entity = _mapper.Map<Council>(dto);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;

            await _uow.Councils.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, CouncilUpdateDto dto)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council ID must be greater than 0", nameof(id));

            // Check if Council exists
            var existing = await _uow.Councils.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Validate Major exists and not deleted (if changed)
            if (existing.MajorId != dto.MajorId)
            {
                var major = await _uow.Majors.GetByIdAsync(dto.MajorId);
                if (major == null)
                    throw new KeyNotFoundException($"Major with ID {dto.MajorId} not found or has been deleted");

                // Check if council has defense sessions - cannot change major if it has sessions
                var defenseSessions = await _uow.DefenseSessions.Query()
                    .Where(ds => ds.CouncilId == id && !ds.IsDeleted)
                    .ToListAsync();
                
                if (defenseSessions.Any())
                    throw new InvalidOperationException($"Cannot change Major for Council {id} because it has {defenseSessions.Count} defense session(s). Please remove defense sessions first or create a new council.");
            }

            // Validate description if provided
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                dto.Description = dto.Description.Trim();
                if (dto.Description.Length < 10)
                    throw new ArgumentException("Council description must be at least 10 characters if provided");
            }

            // Validate IsActive transition
            if (existing.IsActive && !dto.IsActive)
            {
                // Deactivating council - check for active defense sessions
                var activeSessions = await _uow.DefenseSessions.Query()
                    .Where(ds => ds.CouncilId == id && 
                                !ds.IsDeleted && 
                                ds.Status != "Completed" && 
                                ds.Status != "Cancelled")
                    .ToListAsync();

                if (activeSessions.Any())
                    throw new InvalidOperationException($"Cannot deactivate Council {id} because it has {activeSessions.Count} active defense session(s). Please complete or cancel these sessions first.");
            }

            _mapper.Map(dto, existing);
            await _uow.Councils.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council ID must be greater than 0", nameof(id));

            var existing = await _uow.Councils.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Check if council has committee assignments
            var committeeAssignments = await _uow.CommitteeAssignments.Query()
                .Where(ca => ca.CouncilId == id && !ca.IsDeleted)
                .ToListAsync();

            if (committeeAssignments.Any())
                throw new InvalidOperationException($"Cannot delete Council {id} because it has {committeeAssignments.Count} committee assignment(s). Please remove committee members first.");

            // Check if council has defense sessions
            var defenseSessions = await _uow.DefenseSessions.Query()
                .Where(ds => ds.CouncilId == id && !ds.IsDeleted)
                .ToListAsync();

            if (defenseSessions.Any())
                throw new InvalidOperationException($"Cannot delete Council {id} because it has {defenseSessions.Count} defense session(s). Please remove defense sessions first.");

            await _uow.Councils.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council ID must be greater than 0", nameof(id));

            var existing = await _uow.Councils.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Check if already active
            if (existing.IsActive)
                throw new InvalidOperationException($"Council {id} is already active");

            // Validate that Major still exists and is not deleted
            var major = await _uow.Majors.GetByIdAsync(existing.MajorId);
            if (major == null)
                throw new InvalidOperationException($"Cannot restore Council because Major with ID {existing.MajorId} no longer exists or has been deleted");

            await _uow.Councils.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        // Import methods
        public async Task<CouncilCommitteeImportResultDto> ImportCouncilsWithCommitteesAsync(int majorId, IFormFile file)
        {
            var result = new CouncilCommitteeImportResultDto();

            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                throw new ArgumentException("File must be an Excel file (.xlsx or .xls)");

            // Validate majorId
            if (majorId <= 0)
                throw new ArgumentException("Major ID must be greater than 0", nameof(majorId));

            var major = await _uow.Majors.GetByIdAsync(majorId);
            if (major == null)
                throw new KeyNotFoundException($"Major with ID {majorId} not found or has been deleted");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet == null)
                throw new ArgumentException("Excel file has no worksheets");

            // Expected headers
            var expectedHeaders = new[] { "Mã đề tài", "Tên đề tài Tiếng Anh/ Tiếng Nhật", "Tên đề tài Tiếng Việt", "GVHD", "Ngày BVKL", "Giờ", "Hội đồng", "Địa điểm", "Nhiệm vụ TVHĐ", "Họ và tên TVHĐ", "Email" };
            var actualHeaders = new List<string>();
            
            for (int col = 1; col <= 11; col++)
                actualHeaders.Add(worksheet.Cells[1, col].Text?.Trim() ?? "");

            if (!expectedHeaders.SequenceEqual(actualHeaders))
                throw new ArgumentException($"Invalid Excel template. Expected headers: {string.Join(", ", expectedHeaders)}");

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            result.TotalRows = rowCount - 1;

            // Dictionary to track created councils (key: councilId from Excel)
            var createdCouncils = new Dictionary<string, int>();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var councilIdStr = worksheet.Cells[row, 7].Text?.Trim(); // Hội đồng
                    var roleName = worksheet.Cells[row, 9].Text?.Trim(); // Nhiệm vụ TVHĐ
                    var lecturerName = worksheet.Cells[row, 10].Text?.Trim(); // Họ và tên TVHĐ

                    // Validate required fields
                    if (string.IsNullOrEmpty(councilIdStr) || string.IsNullOrEmpty(roleName) || string.IsNullOrEmpty(lecturerName))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Required",
                            ErrorMessage = "Council ID, Role Name, and Lecturer Name are required",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Parse Council ID
                    if (!int.TryParse(councilIdStr, out int councilIdFromExcel))
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

                    // Step 1: Create or get council
                    int actualCouncilId;
                    if (createdCouncils.ContainsKey(councilIdStr))
                    {
                        actualCouncilId = createdCouncils[councilIdStr];
                    }
                    else
                    {
                        // Check if council already exists
                        var existingCouncil = await _uow.Councils.GetByIdAsync(councilIdFromExcel);
                        if (existingCouncil == null)
                        {
                            // Create new council
                            var council = new Council
                            {
                                MajorId = majorId,
                                Description = $"Council {councilIdFromExcel}",
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            };

                            await _uow.Councils.AddAsync(council);
                            await _uow.SaveChangesAsync(); // Save to get the ID

                            createdCouncils[councilIdStr] = council.Id;
                            actualCouncilId = council.Id;
                            result.CreatedCouncilIds.Add(council.Id);
                        }
                        else
                        {
                            // Validate that existing council belongs to the same major
                            if (existingCouncil.MajorId != majorId)
                            {
                                result.Errors.Add(new ImportErrorDto
                                {
                                    Row = row,
                                    Field = "CouncilId",
                                    ErrorMessage = $"Council {councilIdFromExcel} already exists but belongs to different Major (ID: {existingCouncil.MajorId})",
                                    Value = councilIdStr
                                });
                                result.FailureCount++;
                                continue;
                            }

                            actualCouncilId = existingCouncil.Id;
                            createdCouncils[councilIdStr] = actualCouncilId;
                        }
                    }

                    // Step 2: Get Lecturer by FullName
                    var lecturer = await _uow.Lecturers.GetByFullNameAsync(lecturerName);
                    if (lecturer == null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "LecturerName",
                            ErrorMessage = "DEF404: Lecturer not found with this full name",
                            Value = lecturerName
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Step 3: Get CouncilRole by RoleName
                    var councilRole = await _uow.CouncilRoles.GetByRoleNameAsync(roleName);
                    if (councilRole == null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "RoleName",
                            ErrorMessage = "DEF404: Council role not found with this role name",
                            Value = roleName
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Step 4: Check for duplicate committee assignment
                    var existingAssignment = await _uow.CommitteeAssignments.Query()
                        .Where(ca => ca.LecturerId == lecturer.Id && 
                                    ca.CouncilId == actualCouncilId &&
                                    !ca.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (existingAssignment != null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Duplicate",
                            ErrorMessage = $"Lecturer '{lecturerName}' is already assigned to Council {councilIdStr}",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Step 5: Create CommitteeAssignment
                    var committeeAssignment = new CommitteeAssignment
                    {
                        Id = Guid.NewGuid().ToString(),
                        LecturerId = lecturer.Id,
                        CouncilId = actualCouncilId,
                        CouncilRoleId = councilRole.Id,
                        IsDeleted = false
                    };

                    await _uow.CommitteeAssignments.AddAsync(committeeAssignment);
                    result.SuccessCount++;
                    result.CreatedCommitteeAssignmentIds.Add(committeeAssignment.Id);
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

            result.Message = $"Import completed. {result.SuccessCount} committee assignments created successfully, {result.FailureCount} failed. Created {result.CreatedCouncilIds.Count} councils.";
            return result;
        }

        public byte[] GenerateCouncilCommitteeTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Councils_Committees");

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
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
            }

            // Sample data - Council 101
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

            worksheet.Cells[3, 1].Value = "SU25SE053";
            worksheet.Cells[3, 2].Value = "Child Growth and Vaccination Tracking Platform";
            worksheet.Cells[3, 3].Value = "Nền tảng theo dõi tăng trưởng và tiêm chủng của trẻ em";
            worksheet.Cells[3, 4].Value = "Đỗ Tấn Nhân";
            worksheet.Cells[3, 5].Value = "10/3/2025";
            worksheet.Cells[3, 6].Value = "17h30-19h00";
            worksheet.Cells[3, 7].Value = "101";
            worksheet.Cells[3, 8].Value = "NVH 601-Cơ sở NVH";
            worksheet.Cells[3, 9].Value = "Thư ký";
            worksheet.Cells[3, 10].Value = "Lại Đức Hùng";
            worksheet.Cells[3, 11].Value = "HungLD5@fe.edu.vn";

            // Add helpful comments
            worksheet.Cells[1, 7].AddComment("Council ID - same ID will group committee members together", "System");
            worksheet.Cells[1, 9].AddComment("Role name must exist in CouncilRole table (e.g., Chủ tịch HĐ, Thư ký, Thành viên HĐ)", "System");
            worksheet.Cells[1, 10].AddComment("Lecturer full name must exist in the system", "System");

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
