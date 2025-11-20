using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Dto.Student;
using AIDefCom.Service.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.StudentService
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public StudentService(IUnitOfWork uow, IMapper mapper, UserManager<AppUser> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            ExcelHelper.ConfigureExcelPackage();
        }

        public async Task<IEnumerable<StudentReadDto>> GetAllAsync()
        {
            var list = await _uow.Students.GetAllAsync();
            return list.Select(s => new StudentReadDto
            {
                Id = s.Id,
                UserName = s.FullName,
                Email = s.Email,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender
            });
        }

        public async Task<StudentReadDto?> GetByIdAsync(string id)
        {
            var s = await _uow.Students.GetByIdAsync(id);
            return s == null ? null : _mapper.Map<StudentReadDto>(s);
        }

        public async Task<IEnumerable<StudentReadDto>> GetByGroupIdAsync(string groupId)
        {
            var list = await _uow.Students.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<StudentReadDto>>(list);
        }

        public async Task<IEnumerable<StudentReadDto>> GetByUserIdAsync(string userId)
        {
            var list = await _uow.Students.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<StudentReadDto>>(list);
        }

        public async Task<string> AddAsync(StudentCreateDto dto)
        {
            var entity = _mapper.Map<Student>(dto);
            entity.Id = Guid.NewGuid().ToString();
            await _uow.Students.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, StudentUpdateDto dto)
        {
            var existing = await _uow.Students.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Students.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _uow.Students.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.Students.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        // Import methods
        public async Task<ImportResultDto> ImportFromExcelAsync(IFormFile file)
        {
            var result = new ImportResultDto();

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

            // Validate headers - StudentCode is now first
            var expectedHeaders = new[] { "StudentCode", "UserName", "Email", "FullName", "DateOfBirth", "Gender", "PhoneNumber" };
            var actualHeaders = new List<string>();
            
            for (int col = 1; col <= 7; col++)
                actualHeaders.Add(worksheet.Cells[1, col].Text?.Trim() ?? "");

            if (!expectedHeaders.SequenceEqual(actualHeaders))
                throw new ArgumentException($"Invalid Excel template. Expected headers: {string.Join(", ", expectedHeaders)}");

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            result.TotalRows = rowCount - 1;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var studentCode = worksheet.Cells[row, 1].Text?.Trim();
                    var userName = worksheet.Cells[row, 2].Text?.Trim();
                    var email = worksheet.Cells[row, 3].Text?.Trim();
                    var fullName = worksheet.Cells[row, 4].Text?.Trim();
                    var dateOfBirthStr = worksheet.Cells[row, 5].Text?.Trim();
                    var gender = worksheet.Cells[row, 6].Text?.Trim();
                    var phoneNumber = worksheet.Cells[row, 7].Text?.Trim();

                    // Validate required fields
                    if (string.IsNullOrEmpty(studentCode))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "StudentCode",
                            ErrorMessage = "StudentCode is required",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Required",
                            ErrorMessage = "UserName, Email, and FullName are required",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check if StudentCode already exists
                    var existingStudent = await _uow.Students.GetByIdAsync(studentCode);
                    if (existingStudent != null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "StudentCode",
                            ErrorMessage = "StudentCode already exists",
                            Value = studentCode
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check if UserName already exists
                    var existingUser = await _userManager.FindByNameAsync(userName);
                    if (existingUser != null)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "UserName",
                            ErrorMessage = "UserName already exists",
                            Value = userName
                        });
                        result.FailureCount++;
                        continue;
                    }

                    DateTime? dateOfBirth = null;
                    if (!string.IsNullOrEmpty(dateOfBirthStr))
                    {
                        // Try parsing with multiple date formats
                        var dateFormats = new[]
                        {
                            "dd/MM/yyyy",     // 14/03/2004 (Vietnamese format)
                            "MM/dd/yyyy",     // 03/14/2004 (US format)
                            "yyyy-MM-dd",     // 2004-03-14 (ISO format)
                            "dd-MM-yyyy",     // 14-03-2004
                            "MM-dd-yyyy",     // 03-14-2004
                            "d/M/yyyy",       // 14/3/2004 (without leading zeros)
                            "M/d/yyyy"        // 3/14/2004 (without leading zeros)
                        };

                        if (DateTime.TryParseExact(dateOfBirthStr, dateFormats, 
                            System.Globalization.CultureInfo.InvariantCulture, 
                            System.Globalization.DateTimeStyles.None, out var dob))
                        {
                            dateOfBirth = dob;
                        }
                        else if (DateTime.TryParse(dateOfBirthStr, out dob))
                        {
                            // Fallback to default parsing
                            dateOfBirth = dob;
                        }
                        else
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "DateOfBirth",
                                ErrorMessage = "Invalid date format. Use dd/MM/yyyy (e.g., 14/03/2004)",
                                Value = dateOfBirthStr
                            });
                            result.FailureCount++;
                            continue;
                        }
                    }

                    var student = new Student
                    {
                        Id = studentCode, // Use StudentCode as ID instead of Guid
                        UserName = userName,
                        Email = email,
                        FullName = fullName,
                        DateOfBirth = dateOfBirth,
                        Gender = gender,
                        PhoneNumber = phoneNumber,
                        EmailConfirmed = true,
                        HasGeneratedPassword = true,
                        PasswordGeneratedAt = DateTime.UtcNow
                    };

                    var password = GenerateRandomPassword();
                    student.LastGeneratedPassword = password;

                    var createResult = await _userManager.CreateAsync(student, password);

                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(student, "Student");
                        result.SuccessCount++;
                        result.CreatedUserIds.Add(student.Id);
                    }
                    else
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "General",
                            ErrorMessage = string.Join(", ", createResult.Errors.Select(e => e.Description)),
                            Value = userName
                        });
                        result.FailureCount++;
                    }
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

            return result;
        }

        public byte[] GenerateExcelTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");

            // Headers - StudentCode is now the first column
            worksheet.Cells[1, 1].Value = "StudentCode";
            worksheet.Cells[1, 2].Value = "UserName";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "FullName";
            worksheet.Cells[1, 5].Value = "DateOfBirth";
            worksheet.Cells[1, 6].Value = "Gender";
            worksheet.Cells[1, 7].Value = "PhoneNumber";

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Sample data
            worksheet.Cells[2, 1].Value = "SV2024001";
            worksheet.Cells[2, 2].Value = "student001";
            worksheet.Cells[2, 3].Value = "student001@university.edu.vn";
            worksheet.Cells[2, 4].Value = "Nguyen Van A";
            worksheet.Cells[2, 5].Value = "15/01/2002";  // dd/MM/yyyy format
            worksheet.Cells[2, 6].Value = "Male";
            worksheet.Cells[2, 7].Value = "0123456789";

            // Add helpful comments
            worksheet.Cells[1, 1].AddComment("Unique student code (e.g., SV2024001). This will be used as the student ID.", "System");
            worksheet.Cells[1, 2].AddComment("Username can contain letters, numbers, spaces, and special characters (-, ., _, @, +)", "System");
            worksheet.Cells[1, 5].AddComment("Format: dd/MM/yyyy (e.g., 15/01/2002)", "System");

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] GenerateStudentGroupTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students_Groups");

            // Headers theo format yêu cầu
            worksheet.Cells[1, 1].Value = "MSSV";
            worksheet.Cells[1, 2].Value = "Fullname";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Role in group";
            worksheet.Cells[1, 5].Value = "Mã nhóm";
            worksheet.Cells[1, 6].Value = "Mã đề tài";
            worksheet.Cells[1, 7].Value = "Tên đề tài Tiếng Anh/ Tiếng Nhật";
            worksheet.Cells[1, 8].Value = "Tên đề tài Tiếng Việt";

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }

            // Sample data - Group 1
            worksheet.Cells[2, 1].Value = "SE17350";
            worksheet.Cells[2, 2].Value = "Nguyen Duy";
            worksheet.Cells[2, 3].Value = "duynse173501@fpt.edu.vn";
            worksheet.Cells[2, 4].Value = "Leader";
            worksheet.Cells[2, 5].Value = "GFA25SE01";
            worksheet.Cells[2, 6].Value = "FA25SE135";
            worksheet.Cells[2, 7].Value = "Fusion - Multi-Enterprise IT Project Maintenance & Development Platform";
            worksheet.Cells[2, 8].Value = "Nền tảng quản trị bảo trì và phát triển dự án công nghệ thông tin đa doanh nghiệp";

            worksheet.Cells[3, 1].Value = "SE17319";
            worksheet.Cells[3, 2].Value = "Cao Văn Dũng";
            worksheet.Cells[3, 3].Value = "duynse173501@fpt.edu.vn";
            worksheet.Cells[3, 4].Value = "Member";
            worksheet.Cells[3, 5].Value = "GFA25SE01";
            worksheet.Cells[3, 6].Value = "FA25SE135";
            worksheet.Cells[3, 7].Value = "Fusion - Multi-Enterprise IT Project Maintenance & Development Platform";
            worksheet.Cells[3, 8].Value = "Nền tảng quản trị bảo trì và phát triển dự án công nghệ thông tin đa doanh nghiệp";

            // Add helpful comments
            worksheet.Cells[1, 1].AddComment("Student code (MSSV) - will be used as student ID", "System");
            worksheet.Cells[1, 4].AddComment("Role: Leader or Member", "System");
            worksheet.Cells[1, 5].AddComment("Group code (Mã nhóm) - will be used as group ID", "System");

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public async Task<StudentGroupImportResultDto> ImportStudentsWithGroupsAsync(int semesterId, int majorId, IFormFile file)
        {
            var result = new StudentGroupImportResultDto();

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

            // Validate headers
            var expectedHeaders = new[] { "MSSV", "Fullname", "Email", "Role in group", "Mã nhóm", "Mã đề tài", "Tên đề tài Tiếng Anh/ Tiếng Nhật", "Tên đề tài Tiếng Việt" };
            var actualHeaders = new List<string>();
            
            for (int col = 1; col <= 8; col++)
                actualHeaders.Add(worksheet.Cells[1, col].Text?.Trim() ?? "");

            if (!expectedHeaders.SequenceEqual(actualHeaders))
                throw new ArgumentException($"Invalid Excel template. Expected headers: {string.Join(", ", expectedHeaders)}");

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            result.TotalRows = rowCount - 1;

            // Dictionary để track groups đã tạo (key: groupCode)
            var createdGroups = new Dictionary<string, string>();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var mssv = worksheet.Cells[row, 1].Text?.Trim();
                    var fullName = worksheet.Cells[row, 2].Text?.Trim();
                    var email = worksheet.Cells[row, 3].Text?.Trim();
                    var roleInGroup = worksheet.Cells[row, 4].Text?.Trim();
                    var groupCode = worksheet.Cells[row, 5].Text?.Trim();
                    var projectCode = worksheet.Cells[row, 6].Text?.Trim();
                    var topicTitleEN = worksheet.Cells[row, 7].Text?.Trim();
                    var topicTitleVN = worksheet.Cells[row, 8].Text?.Trim();

                    // Validate required fields
                    if (string.IsNullOrEmpty(mssv) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                        string.IsNullOrEmpty(groupCode) || string.IsNullOrEmpty(projectCode))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Required",
                            ErrorMessage = "MSSV, Fullname, Email, Group Code, and Project Code are required",
                            Value = ""
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Step 1: Create or get student
                    var existingStudent = await _uow.Students.GetByIdAsync(mssv);
                    if (existingStudent == null)
                    {
                        // Create new student
                        var student = new Student
                        {
                            Id = mssv,
                            UserName = email, // Username = Email
                            Email = email,
                            FullName = fullName,
                            DateOfBirth = null, // Để trống
                            Gender = null, // Để trống
                            PhoneNumber = null, // Để trống
                            EmailConfirmed = true,
                            HasGeneratedPassword = true,
                            PasswordGeneratedAt = DateTime.UtcNow
                        };

                        var password = GenerateRandomPassword();
                        student.LastGeneratedPassword = password;

                        var createResult = await _userManager.CreateAsync(student, password);

                        if (createResult.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(student, "Student");
                            result.CreatedStudentIds.Add(student.Id);
                        }
                        else
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Student",
                                ErrorMessage = string.Join(", ", createResult.Errors.Select(e => e.Description)),
                                Value = mssv
                            });
                            result.FailureCount++;
                            continue;
                        }
                    }

                    // Step 2: Create or get group
                    string groupId;
                    if (createdGroups.ContainsKey(groupCode))
                    {
                        groupId = createdGroups[groupCode];
                    }
                    else
                    {
                        var existingGroup = await _uow.Groups.GetByIdAsync(groupCode);
                        if (existingGroup == null)
                        {
                            // Create new group
                            var group = new Group
                            {
                                Id = groupCode,
                                ProjectCode = projectCode,
                                TopicTitle_EN = topicTitleEN ?? "",
                                TopicTitle_VN = topicTitleVN ?? "",
                                SemesterId = semesterId,
                                MajorId = majorId,
                                Status = "Active"
                            };

                            await _uow.Groups.AddAsync(group);
                            createdGroups[groupCode] = groupCode;
                            groupId = groupCode;
                            result.CreatedGroupIds.Add(groupId);
                        }
                        else
                        {
                            groupId = existingGroup.Id;
                            createdGroups[groupCode] = groupId;
                        }
                    }

                    // Step 3: Create StudentGroup relationship
                    var studentGroup = new StudentGroup
                    {
                        UserId = mssv,
                        GroupId = groupId,
                        GroupRole = roleInGroup
                    };

                    await _uow.StudentGroups.AddAsync(studentGroup);
                    result.SuccessCount++;
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

            result.Message = $"Import completed. {result.SuccessCount} records processed successfully, {result.FailureCount} failed. Created {result.CreatedStudentIds.Count} students and {result.CreatedGroupIds.Count} groups.";
            return result;
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
