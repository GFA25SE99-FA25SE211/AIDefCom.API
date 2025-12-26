using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Dto.Student;
using AIDefCom.Service.Helpers;
using AIDefCom.Service.Services.EmailService;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEmailService _emailService;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IUnitOfWork uow, 
            IMapper mapper, 
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ILogger<StudentService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            ExcelHelper.ConfigureExcelPackage();
        }

        public async Task<IEnumerable<StudentReadDto>> GetAllAsync()
        {
            var list = await _uow.Students.GetAllAsync();
            
            // Lấy tất cả StudentGroup để mapping GroupId và GroupRole
            var allStudentGroups = await _uow.StudentGroups.GetAllAsync();
            
            return list.Select(s => 
            {
                var studentGroup = allStudentGroups.FirstOrDefault(sg => sg.UserId == s.Id);
                return new StudentReadDto
                {
                    Id = s.Id,
                    UserName = s.FullName,
                    Email = s.Email,
                    DateOfBirth = s.DateOfBirth,
                    Gender = s.Gender,
                    GroupId = studentGroup?.GroupId,
                    GroupRole = studentGroup?.GroupRole
                };
            });
        }

        public async Task<StudentReadDto?> GetByIdAsync(string id)
        {
            var s = await _uow.Students.GetByIdAsync(id);
            return s == null ? null : _mapper.Map<StudentReadDto>(s);
        }

        public async Task<IEnumerable<StudentReadDto>> GetByGroupIdAsync(string groupId)
        {
            // Lấy StudentGroups để có GroupRole
            var studentGroups = await _uow.StudentGroups.GetByGroupIdAsync(groupId);
            
            // Lấy danh sách student IDs
            var studentIds = studentGroups.Select(sg => sg.UserId).ToList();
            
            // Lấy students
            var students = await _uow.Students.Query()
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();
            
            // Map kết quả và gắn GroupRole từ StudentGroup
            return students.Select(s => 
            {
                var studentGroup = studentGroups.FirstOrDefault(sg => sg.UserId == s.Id);
                return new StudentReadDto
                {
                    Id = s.Id,
                    UserName = s.FullName,
                    Email = s.Email,
                    DateOfBirth = s.DateOfBirth,
                    Gender = s.Gender,
                    GroupId = groupId,
                    GroupRole = studentGroup?.GroupRole
                };
            });
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

        // Import methods - All or Nothing approach
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

            if (rowCount < 2)
                throw new ArgumentException("Excel file contains no data rows");

            // ✅ PHASE 1: VALIDATE ALL ROWS (No database operations)
            var validatedStudents = new List<(int Row, Student Student, string Password)>();
            var processedStudentCodes = new HashSet<string>();
            var processedUserNames = new HashSet<string>();
            var processedEmails = new HashSet<string>();

            for (int row = 2; row <= rowCount; row++)
            {
                var studentCode = worksheet.Cells[row, 1].Text?.Trim();
                var userName = worksheet.Cells[row, 2].Text?.Trim();
                var email = worksheet.Cells[row, 3].Text?.Trim();
                var fullName = worksheet.Cells[row, 4].Text?.Trim();
                var dateOfBirthStr = worksheet.Cells[row, 5].Text?.Trim();
                var gender = worksheet.Cells[row, 6].Text?.Trim();
                var phoneNumber = worksheet.Cells[row, 7].Text?.Trim();

                // Validation 1: Required fields
                if (string.IsNullOrEmpty(studentCode))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "StudentCode",
                        ErrorMessage = "StudentCode is required",
                        Value = ""
                    });
                    continue;
                }

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "UserName, Email, FullName",
                        ErrorMessage = "UserName, Email, and FullName are required",
                        Value = $"{userName}, {email}, {fullName}"
                    });
                    continue;
                }

                // Validation 2: Email format
                if (!email.Contains("@") || !email.Contains("."))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = "Invalid email format",
                        Value = email
                    });
                    continue;
                }

                // Validation 3: Duplicate StudentCode within file
                if (processedStudentCodes.Contains(studentCode))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "StudentCode",
                        ErrorMessage = $"Duplicate StudentCode '{studentCode}' found in the file",
                        Value = studentCode
                    });
                    continue;
                }

                // Validation 4: Duplicate UserName within file
                if (processedUserNames.Contains(userName.ToLower()))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "UserName",
                        ErrorMessage = $"Duplicate UserName '{userName}' found in the file",
                        Value = userName
                    });
                    continue;
                }

                // Validation 5: Duplicate Email within file
                if (processedEmails.Contains(email.ToLower()))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = $"Duplicate Email '{email}' found in the file",
                        Value = email
                    });
                    continue;
                }

                // Validation 6: Check if StudentCode already exists in database
                var existingStudent = await _uow.Students.GetByIdAsync(studentCode);
                if (existingStudent != null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "StudentCode",
                        ErrorMessage = $"StudentCode '{studentCode}' already exists in the system",
                        Value = studentCode
                    });
                    continue;
                }

                // Validation 7: Check if UserName already exists in database
                var existingUserByName = await _userManager.FindByNameAsync(userName);
                if (existingUserByName != null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "UserName",
                        ErrorMessage = $"UserName '{userName}' already exists in the system",
                        Value = userName
                    });
                    continue;
                }

                // Validation 8: Check if Email already exists in database
                var existingUserByEmail = await _userManager.FindByEmailAsync(email);
                if (existingUserByEmail != null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = $"Email '{email}' already exists in the system",
                        Value = email
                    });
                    continue;
                }

                // Validation 9: Date format
                DateTime? dateOfBirth = null;
                if (!string.IsNullOrEmpty(dateOfBirthStr))
                {
                    var dateFormats = new[]
                    {
                        "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd",
                        "dd-MM-yyyy", "MM-dd-yyyy", "d/M/yyyy", "M/d/yyyy"
                    };

                    if (!DateTime.TryParseExact(dateOfBirthStr, dateFormats, 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out var dob) &&
                        !DateTime.TryParse(dateOfBirthStr, out dob))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "DateOfBirth",
                            ErrorMessage = "Invalid date format. Use dd/MM/yyyy (e.g., 14/03/2004)",
                            Value = dateOfBirthStr
                        });
                        continue;
                    }
                    dateOfBirth = dob;
                }

                // All validations passed - prepare student object
                var student = new Student
                {
                    Id = studentCode,
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

                validatedStudents.Add((row, student, password));
                processedStudentCodes.Add(studentCode);
                processedUserNames.Add(userName.ToLower());
                processedEmails.Add(email.ToLower());
            }

            // ✅ PHASE 2: CHECK IF ANY VALIDATION ERRORS
            if (result.Errors.Any())
            {
                result.FailureCount = result.Errors.Count;
                result.TotalRows = rowCount - 1;
                throw new ArgumentException($"Import validation failed. Found {result.Errors.Count} error(s). Please fix all errors and try again. Errors: {string.Join("; ", result.Errors.Select(e => $"Row {e.Row}: {e.ErrorMessage} (Field: {e.Field}, Value: '{e.Value}')"))}");
            }

            // ✅ PHASE 3: ALL VALID - INSERT ALL (Transaction)
            try
            {
                foreach (var (row, student, password) in validatedStudents)
                {
                    var createResult = await _userManager.CreateAsync(student, password);

                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Row {row}: Failed to create student '{student.UserName}'. {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    await _userManager.AddToRoleAsync(student, "Student");
                    result.CreatedUserIds.Add(student.Id);

                    // Send email (non-blocking)
                    try
                    {
                        await SendWelcomeEmail(student.Email, student.UserName, password, student.FullName);
                        _logger.LogInformation("✅ Sent welcome email to {Email}", student.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning("⚠️ Failed to send welcome email to {Email}: {Error}", 
                            student.Email, emailEx.Message);
                    }
                }

                result.SuccessCount = validatedStudents.Count;
                result.FailureCount = 0;
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Import failed: {ex.Message}. All changes have been rolled back.", ex);
            }
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

            try
            {
                // ✅ VALIDATION 1: Parameters validation
                if (semesterId <= 0)
                    throw new ArgumentException("Semester ID must be greater than 0");
                if (majorId <= 0)
                    throw new ArgumentException("Major ID must be greater than 0");

                // ✅ VALIDATION 2: Verify Semester exists
                var semester = await _uow.Semesters.GetByIdAsync(semesterId);
                if (semester == null)
                    throw new ArgumentException($"Semester with ID {semesterId} not found");

                // ✅ VALIDATION 3: Verify Major exists
                var major = await _uow.Majors.GetByIdAsync(majorId);
                if (major == null)
                    throw new ArgumentException($"Major with ID {majorId} not found");

                // ✅ VALIDATION 4-10: File validations (same as ImportFromExcelAsync)
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File cannot be null or empty. Please upload a valid Excel file.");

                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                    throw new ArgumentException($"File size exceeds the maximum allowed size of {maxFileSize / (1024 * 1024)}MB. Current file size: {file.Length / (1024.0 * 1024):F2}MB");

                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                    throw new ArgumentException($"Invalid file type. Only Excel files (.xlsx, .xls) are allowed. Current file type: {fileExtension}");

                if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
                    throw new ArgumentException("Invalid file name. File name cannot contain special characters (/, \\, ..)");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                
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
                    if (package.Workbook.Worksheets.Count == 0)
                        throw new ArgumentException("Excel file contains no worksheets. Please use the standard template.");

                    var worksheet = package.Workbook.Worksheets[0];
                    
                    if (worksheet == null)
                        throw new ArgumentException("Unable to read the first worksheet in the Excel file.");

                    if (worksheet.Dimension == null || worksheet.Dimension.Rows < 2)
                        throw new ArgumentException("Excel file contains no data. At least 1 header row and 1 data row are required.");

                    // ✅ VALIDATION: Headers validation
                    var expectedHeaders = new[] { "MSSV", "Fullname", "Email", "Role in group", "Mã nhóm", "Mã đề tài", "Tên đề tài Tiếng Anh/ Tiếng Nhật", "Tên đề tài Tiếng Việt" };
                    var actualHeaders = new List<string>();
                    
                    for (int col = 1; col <= 8; col++)
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

                    int rowCount = worksheet.Dimension.Rows;
                    const int maxRows = 1000;
                    if (rowCount > maxRows + 1)
                        throw new ArgumentException($"File exceeds the maximum allowed rows ({maxRows} data rows). Current file has {rowCount - 1} data rows.");

                    result.TotalRows = rowCount - 1;

                    if (rowCount < 2)
                        throw new ArgumentException("Excel file contains no data rows. At least 1 data row is required after the header.");

                    return await ProcessStudentGroupImportRows(worksheet, rowCount, result, semesterId, majorId);
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

        private async Task<StudentGroupImportResultDto> ProcessStudentGroupImportRows(
            ExcelWorksheet worksheet, int rowCount, StudentGroupImportResultDto result, int semesterId, int majorId)
        {
            // ✅ PHASE 1: VALIDATE ALL ROWS
            var validatedData = new List<(
                int Row,
                string Mssv,
                string FullName,
                string Email,
                string RoleInGroup,
                string GroupCode,
                string ProjectCode,
                string TopicTitleEN,
                string TopicTitleVN,
                string Password
            )>();
            
            var processedStudents = new HashSet<string>();
            var processedEmails = new HashSet<string>();
            var groupData = new Dictionary<string, (string ProjectCode, string TopicEN, string TopicVN)>();

            for (int row = 2; row <= rowCount; row++)
            {
                var mssv = worksheet.Cells[row, 1].Text?.Trim();
                var fullName = worksheet.Cells[row, 2].Text?.Trim();
                var email = worksheet.Cells[row, 3].Text?.Trim();
                var roleInGroup = worksheet.Cells[row, 4].Text?.Trim();
                var groupCode = worksheet.Cells[row, 5].Text?.Trim();
                var projectCode = worksheet.Cells[row, 6].Text?.Trim();
                var topicTitleEN = worksheet.Cells[row, 7].Text?.Trim();
                var topicTitleVN = worksheet.Cells[row, 8].Text?.Trim();

                // BỎ QUA DÒNG TRỐNG
                if (string.IsNullOrEmpty(mssv) &&
                    string.IsNullOrEmpty(fullName) &&
                    string.IsNullOrEmpty(email) &&
                    string.IsNullOrEmpty(roleInGroup) &&
                    string.IsNullOrEmpty(groupCode) &&
                    string.IsNullOrEmpty(projectCode) &&
                    string.IsNullOrEmpty(topicTitleEN) &&
                    string.IsNullOrEmpty(topicTitleVN))
                {
                    continue;
                }

                // ✅ VALIDATION: Required fields
                var missingFields = new List<string>();
                if (string.IsNullOrEmpty(mssv)) missingFields.Add("MSSV");
                if (string.IsNullOrEmpty(fullName)) missingFields.Add("Fullname");
                if (string.IsNullOrEmpty(email)) missingFields.Add("Email");
                if (string.IsNullOrEmpty(groupCode)) missingFields.Add("Mã nhóm");
                if (string.IsNullOrEmpty(projectCode)) missingFields.Add("Mã đề tài");

                if (missingFields.Any())
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = string.Join(", ", missingFields),
                        ErrorMessage = $"Required field(s) missing: {string.Join(", ", missingFields)}",
                        Value = ""
                    });
                    continue;
                }

                // ✅ VALIDATION: Student code length
                if (mssv.Length > 50)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "MSSV",
                        ErrorMessage = "Student code cannot exceed 50 characters",
                        Value = mssv
                    });
                    continue;
                }

                // ✅ VALIDATION: Email format
                if (!email.Contains("@") || !email.Contains("."))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = "Invalid email format",
                        Value = email
                    });
                    continue;
                }

                if (email.Length > 256)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = "Email cannot exceed 256 characters",
                        Value = email
                    });
                    continue;
                }

                // ✅ VALIDATION: Role
                if (!string.IsNullOrEmpty(roleInGroup))
                {
                    var validRoles = new[] { "Leader", "Member", "leader", "member" };
                    if (!validRoles.Contains(roleInGroup))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Role in group",
                            ErrorMessage = $"Invalid role. Allowed values: Leader, Member",
                            Value = roleInGroup
                        });
                        continue;
                    }
                }

                // ✅ VALIDATION: Duplicate MSSV within file
                if (processedStudents.Contains(mssv))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "MSSV",
                        ErrorMessage = $"Duplicate student code '{mssv}' found in the file",
                        Value = mssv
                    });
                    continue;
                }

                // ✅ VALIDATION: Duplicate Email within file
                if (processedEmails.Contains(email.ToLower()))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = $"Duplicate email '{email}' found in the file",
                        Value = email
                    });
                    continue;
                }

                // ✅ VALIDATION: Check if Student exists in database
                var existingStudent = await _uow.Students.GetByIdAsync(mssv);
                if (existingStudent != null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "MSSV",
                        ErrorMessage = $"Student with code '{mssv}' already exists in the system",
                        Value = mssv
                    });
                    continue;
                }

                // ✅ VALIDATION: Check if Email exists in database
                var existingEmail = await _userManager.FindByEmailAsync(email);
                if (existingEmail != null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "Email",
                        ErrorMessage = $"Email '{email}' already exists in the system",
                        Value = email
                    });
                    continue;
                }

                // ✅ VALIDATION: Group consistency
                if (groupData.ContainsKey(groupCode))
                {
                    var existing = groupData[groupCode];
                    if (existing.ProjectCode != projectCode)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = row,
                            Field = "Mã nhóm, Mã đề tài",
                            ErrorMessage = $"Group '{groupCode}' has conflicting project codes in file: '{existing.ProjectCode}' vs '{projectCode}'",
                            Value = $"{groupCode}, {projectCode}"
                        });
                        continue;
                    }
                }
                else
                {
                    // Check if group exists in database
                    var existingGroup = await _uow.Groups.GetByIdAsync(groupCode);
                    if (existingGroup != null)
                    {
                        if (existingGroup.SemesterId != semesterId || existingGroup.MajorId != majorId)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Mã nhóm",
                                ErrorMessage = $"Group '{groupCode}' exists but belongs to different Semester or Major",
                                Value = groupCode
                            });
                            continue;
                        }
                        groupData[groupCode] = (existingGroup.ProjectCode, existingGroup.TopicTitle_EN ?? "", existingGroup.TopicTitle_VN ?? "");
                    }
                    else
                    {
                        // New group - check if projectCode already used
                        var existingGroupByProject = await _uow.Groups.GetByProjectCodeAsync(projectCode);
                        if (existingGroupByProject != null)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Mã đề tài",
                                ErrorMessage = $"Project code '{projectCode}' already exists for group '{existingGroupByProject.Id}'",
                                Value = projectCode
                            });
                            continue;
                        }

                        if (projectCode.Length > 50)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "Mã đề tài",
                                ErrorMessage = "Project code cannot exceed 50 characters",
                                Value = projectCode
                            });
                            continue;
                        }

                        groupData[groupCode] = (projectCode, topicTitleEN ?? "", topicTitleVN ?? "");
                    }
                }

                // ✅ VALIDATION: Check if student already in group
                var existingAssignment = await _uow.StudentGroups.Query()
                    .Where(sg => sg.UserId == mssv && sg.GroupId == groupCode)
                    .FirstOrDefaultAsync();

                if (existingAssignment != null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = row,
                        Field = "MSSV",
                        ErrorMessage = $"Student '{mssv}' is already assigned to group '{groupCode}'",
                        Value = mssv
                    });
                    continue;
                }

                // All validations passed
                var password = GenerateRandomPassword();
                validatedData.Add((row, mssv, fullName, email, roleInGroup, groupCode, projectCode, topicTitleEN ?? "", topicTitleVN ?? "", password));
                processedStudents.Add(mssv);
                processedEmails.Add(email.ToLower());
            }

            // ✅ PHASE 2: CHECK IF ANY VALIDATION ERRORS
            if (result.Errors.Any())
            {
                result.FailureCount = result.Errors.Count;
                result.TotalRows = rowCount - 1;
                throw new ArgumentException($"Import validation failed. Found {result.Errors.Count} error(s). Please fix all errors and try again. Errors: {string.Join("; ", result.Errors.Select(e => $"Row {e.Row}: {e.ErrorMessage} (Field: {e.Field}, Value: '{e.Value}')"))}");
            }

            // ✅ PHASE 3: INSERT ALL (Transaction)
            try
            {
                var createdGroups = new HashSet<string>();

                foreach (var data in validatedData)
                {
                    // Create group if not exists
                    if (!createdGroups.Contains(data.GroupCode))
                    {
                        var existingGroup = await _uow.Groups.GetByIdAsync(data.GroupCode);
                        if (existingGroup == null)
                        {
                            var group = new Group
                            {
                                Id = data.GroupCode,
                                ProjectCode = data.ProjectCode,
                                TopicTitle_EN = data.TopicTitleEN,
                                TopicTitle_VN = data.TopicTitleVN,
                                SemesterId = semesterId,
                                MajorId = majorId,
                                Status = "Active"
                            };

                            await _uow.Groups.AddAsync(group);
                            result.CreatedGroupIds.Add(data.GroupCode);
                        }
                        createdGroups.Add(data.GroupCode);
                    }

                    // Create student
                    var student = new Student
                    {
                        Id = data.Mssv,
                        UserName = data.Email,
                        Email = data.Email,
                        FullName = data.FullName,
                        EmailConfirmed = true,
                        HasGeneratedPassword = true,
                        PasswordGeneratedAt = DateTime.UtcNow,
                        LastGeneratedPassword = data.Password
                    };

                    var createResult = await _userManager.CreateAsync(student, data.Password);
                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Row {data.Row}: Failed to create student '{data.Mssv}'. {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    await _userManager.AddToRoleAsync(student, "Student");
                    result.CreatedStudentIds.Add(student.Id);

                    // Create StudentGroup relationship
                    var studentGroup = new StudentGroup
                    {
                        UserId = data.Mssv,
                        GroupId = data.GroupCode,
                        GroupRole = data.RoleInGroup
                    };

                    await _uow.StudentGroups.AddAsync(studentGroup);

                    // Send email (non-blocking)
                    try
                    {
                        await SendWelcomeEmail(student.Email, student.UserName, data.Password, student.FullName);
                        _logger.LogInformation("✅ Sent welcome email to {Email}", student.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning("⚠️ Failed to send welcome email to {Email}: {Error}", 
                            student.Email, emailEx.Message);
                    }
                }

                await _uow.SaveChangesAsync();

                result.SuccessCount = validatedData.Count;
                result.FailureCount = 0;
                result.Message = $"Import completed successfully. Created {result.CreatedStudentIds.Count} students and {result.CreatedGroupIds.Count} groups.";
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Import failed: {ex.Message}. All changes have been rolled back.", ex);
            }
        }

        private string GenerateRandomPassword()
        {
            // Tạo password đủ mạnh theo yêu cầu: 8-16 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt
            var random = new Random();
            var password = new StringBuilder();
            
            // Đảm bảo có ít nhất 1 chữ hoa
            password.Append("ABCDEFGHJKLMNPQRSTUVWXYZ"[random.Next(23)]);
            
            // Đảm bảo có ít nhất 1 chữ thường
            password.Append("abcdefghjkmnpqrstuvwxyz"[random.Next(23)]);
            
            // Đảm bảo có ít nhất 1 số
            password.Append("23456789"[random.Next(8)]);
            
            // Đảm bảo có ít nhất 1 ký tự đặc biệt
            password.Append("!@#$%^&*"[random.Next(8)]);
            
            // Thêm các ký tự ngẫu nhiên để đủ 12 ký tự
            const string allChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%^&*";
            for (int i = 0; i < 8; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }
            
            // Shuffle password
            var passwordChars = password.ToString().ToCharArray();
            for (int i = passwordChars.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }
            
            return new string(passwordChars);
        }

        /// <summary>
        /// Gửi email chào mừng với thông tin đăng nhập
        /// </summary>
        private async Task SendWelcomeEmail(string email, string username, string password, string fullName)
        {
            var message = new MessageOTP(
                new string[] { email },
                "🎓 Tài Khoản Hệ Thống AIDefCom - Thông Tin Đăng Nhập",
                $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .credentials {{ background-color: #f0f8ff; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0; }}
        .credentials strong {{ color: #2196F3; }}
        .warning {{ background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎓 Chào Mừng Đến Với AIDefCom</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{fullName}</strong>,</p>
            
            <p>Tài khoản của bạn đã được tạo thành công trong hệ thống <strong>AIDefCom</strong> (Hệ thống Quản lý Bảo vệ Đồ án Tốt nghiệp).</p>
            
            <div class=""credentials"">
                <h3>📧 Thông Tin Đăng Nhập</h3>
                <p><strong>Email/Tên đăng nhập:</strong> {username}</p>
                <p><strong>Mật khẩu tạm thời:</strong> <span style=""font-size: 18px; color: #e74c3c; font-weight: bold;"">{password}</span></p>
                <p><strong>URL đăng nhập:</strong> <a href=""https://aidefcom.io.vn"">https://aidefcom.io.vn</a></p>
            </div>
            
            <div class=""warning"">
                <h3>⚠️ Lưu Ý Quan Trọng</h3>
                <ul>
                    <li><strong>Đổi mật khẩu ngay</strong> sau lần đăng nhập đầu tiên</li>
                    <li><strong>KHÔNG chia sẻ</strong> mật khẩu với bất kỳ ai</li>
                    <li>Mật khẩu mới phải có <strong>8-16 ký tự</strong></li>
                    <li>Bao gồm <strong>ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt</strong></li>
                    <li>Nếu quên mật khẩu, sử dụng chức năng ""Quên mật khẩu"" trên trang đăng nhập</li>
                </ul>
            </div>
            
            <p><strong>Hướng dẫn đăng nhập:</strong></p>
            <ol>
                <li>Truy cập <a href=""https://aidefcom.io.vn"">https://aidefcom.io.vn</a></li>
                <li>Nhập email: <strong>{username}</strong></li>
                <li>Nhập mật khẩu: <strong>{password}</strong></li>
                <li>Nhấn ""Đăng nhập""</li>
                <li>Sau khi đăng nhập thành công, vào <strong>Cài đặt → Đổi mật khẩu</strong></li>
            </ol>
            
            <a href=""https://aidefcom.io.vn/login"" class=""button"">🔐 Đăng Nhập Ngay</a>
        </div>
        <div class=""footer"">
            <p>Email này được gửi tự động từ hệ thống AIDefCom</p>
            <p>Nếu bạn có thắc mắc, vui lòng liên hệ quản trị viên hệ thống</p>
            <p>&copy; {DateTime.Now.Year} AIDefCom. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
"
            );

            _emailService.SendEmail(message);
            await Task.CompletedTask;
        }
    }
}
