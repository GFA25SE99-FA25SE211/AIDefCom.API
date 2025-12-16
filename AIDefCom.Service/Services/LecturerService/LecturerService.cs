using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Dto.Lecturer;
using AIDefCom.Service.Helpers;
using AIDefCom.Service.Services.EmailService;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.LecturerService
{
    public class LecturerService : ILecturerService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<LecturerService> _logger;

        public LecturerService(
            IUnitOfWork uow, 
            IMapper mapper, 
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ILogger<LecturerService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            ExcelHelper.ConfigureExcelPackage();
        }

        public async Task<IEnumerable<LecturerReadDto>> GetAllAsync()
        {
            var list = await _uow.Lecturers.GetAllAsync();
            return list.Select(l => new LecturerReadDto
            {
                Id = l.Id,
                FullName = l.FullName,
                Email = l.Email,
                PhoneNumber = l.PhoneNumber,
                DateOfBirth = l.DateOfBirth,
                Gender = l.Gender,
                Department = l.Department,
                AcademicRank = l.AcademicRank,
                Degree = l.Degree
            });
        }

        public async Task<LecturerReadDto?> GetByIdAsync(string id)
        {
            var lecturer = await _uow.Lecturers.GetByIdAsync(id);
            return lecturer == null ? null : _mapper.Map<LecturerReadDto>(lecturer);
        }

        public async Task<IEnumerable<LecturerReadDto>> GetByDepartmentAsync(string department)
        {
            var list = await _uow.Lecturers.GetByDepartmentAsync(department);
            return _mapper.Map<IEnumerable<LecturerReadDto>>(list);
        }

        public async Task<IEnumerable<LecturerReadDto>> GetByAcademicRankAsync(string academicRank)
        {
            var list = await _uow.Lecturers.GetByAcademicRankAsync(academicRank);
            return _mapper.Map<IEnumerable<LecturerReadDto>>(list);
        }

        public async Task<string> AddAsync(LecturerCreateDto dto)
        {
            var entity = _mapper.Map<Lecturer>(dto);
            // Id should be provided from dto (from AppUser creation)
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            await _uow.Lecturers.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, LecturerUpdateDto dto)
        {
            var existing = await _uow.Lecturers.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Lecturers.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _uow.Lecturers.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.Lecturers.DeleteAsync(id);
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

            // Validate headers
            var expectedHeaders = new[] { "UserName", "Email", "FullName", "DateOfBirth", "Gender", "PhoneNumber", "Department", "AcademicRank", "Degree" };
            var actualHeaders = new List<string>();
            
            for (int col = 1; col <= 9; col++)
                actualHeaders.Add(worksheet.Cells[1, col].Text?.Trim() ?? "");

            if (!expectedHeaders.SequenceEqual(actualHeaders))
                throw new ArgumentException($"Invalid Excel template. Expected headers: {string.Join(", ", expectedHeaders)}");

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            result.TotalRows = rowCount - 1;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var userName = worksheet.Cells[row, 1].Text?.Trim();
                    var email = worksheet.Cells[row, 2].Text?.Trim();
                    var fullName = worksheet.Cells[row, 3].Text?.Trim();
                    var dateOfBirthStr = worksheet.Cells[row, 4].Text?.Trim();
                    var gender = worksheet.Cells[row, 5].Text?.Trim();
                    var phoneNumber = worksheet.Cells[row, 6].Text?.Trim();
                    var department = worksheet.Cells[row, 7].Text?.Trim();
                    var academicRank = worksheet.Cells[row, 8].Text?.Trim();
                    var degree = worksheet.Cells[row, 9].Text?.Trim();

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
                            "dd/MM/yyyy",     // 14/03/1980 (Vietnamese format)
                            "MM/dd/yyyy",     // 03/14/1980 (US format)
                            "yyyy-MM-dd",     // 1980-03-14 (ISO format)
                            "dd-MM-yyyy",     // 14-03-1980
                            "MM-dd-yyyy",     // 03-14-1980
                            "d/M/yyyy",       // 14/3/1980 (without leading zeros)
                            "M/d/yyyy"        // 3/14/1980 (without leading zeros)
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
                                ErrorMessage = "Invalid date format. Use dd/MM/yyyy (e.g., 15/01/1980)",
                                Value = dateOfBirthStr
                            });
                            result.FailureCount++;
                            continue;
                        }
                    }

                    var lecturer = new Lecturer
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = userName,
                        Email = email,
                        FullName = fullName,
                        DateOfBirth = dateOfBirth,
                        Gender = gender,
                        PhoneNumber = phoneNumber,
                        Department = department,
                        AcademicRank = academicRank,
                        Degree = degree,
                        EmailConfirmed = true,
                        HasGeneratedPassword = true,
                        PasswordGeneratedAt = DateTime.UtcNow
                    };

                    var password = GenerateRandomPassword();
                    lecturer.LastGeneratedPassword = password;

                    var createResult = await _userManager.CreateAsync(lecturer, password);

                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(lecturer, "Lecturer");
                        result.SuccessCount++;
                        result.CreatedUserIds.Add(lecturer.Id);

                        // 📧 Gửi email thông báo tài khoản và mật khẩu
                        try
                        {
                            await SendWelcomeEmail(lecturer.Email, lecturer.UserName, password, lecturer.FullName);
                            _logger.LogInformation("✅ Sent welcome email to {Email}", lecturer.Email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning("⚠️ Failed to send welcome email to {Email}: {Error}", 
                                lecturer.Email, emailEx.Message);
                            // Không fail import nếu gửi email lỗi, chỉ log warning
                        }
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
            var worksheet = package.Workbook.Worksheets.Add("Lecturers");

            // Headers
            worksheet.Cells[1, 1].Value = "UserName";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "FullName";
            worksheet.Cells[1, 4].Value = "DateOfBirth";
            worksheet.Cells[1, 5].Value = "Gender";
            worksheet.Cells[1, 6].Value = "PhoneNumber";
            worksheet.Cells[1, 7].Value = "Department";
            worksheet.Cells[1, 8].Value = "AcademicRank";
            worksheet.Cells[1, 9].Value = "Degree";

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }

            // Sample data
            worksheet.Cells[2, 1].Value = "lecturer001";
            worksheet.Cells[2, 2].Value = "lecturer001@university.edu.vn";
            worksheet.Cells[2, 3].Value = "Dr. Nguyen Van A";
            worksheet.Cells[2, 4].Value = "15/01/1980";  // dd/MM/yyyy format
            worksheet.Cells[2, 5].Value = "Male";
            worksheet.Cells[2, 6].Value = "0123456789";
            worksheet.Cells[2, 7].Value = "Computer Science";
            worksheet.Cells[2, 8].Value = "Associate Professor";
            worksheet.Cells[2, 9].Value = "PhD";

            // Add helpful comments
            worksheet.Cells[1, 1].AddComment("Username can contain letters, numbers, spaces, and special characters (-, ., _, @, +)", "System");
            worksheet.Cells[1, 4].AddComment("Format: dd/MM/yyyy (e.g., 15/01/1980)", "System");

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Gửi email chào mừng với thông tin đăng nhập (cho Giảng viên)
        /// </summary>
        private async Task SendWelcomeEmail(string email, string username, string password, string fullName)
        {
            var message = new MessageOTP(
                new string[] { email },
                "🎓 Tài Khoản Hệ Thống AIDefCom - Thông Tin Đăng Nhập (Giảng Viên)",
                $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .credentials {{ background-color: #f0f8ff; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0; }}
        .credentials strong {{ color: #2196F3; }}
        .warning {{ background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎓 Chào Mừng Giảng Viên - AIDefCom</h1>
        </div>
        <div class=""content"">
            <p>Kính chào <strong>{fullName}</strong>,</p>
            
            <p>Tài khoản của Quý Thầy/Cô đã được tạo thành công trong hệ thống <strong>AIDefCom</strong> (Hệ thống Quản lý Bảo vệ Đồ án Tốt nghiệp).</p>
            
            <div class=""credentials"">
                <h3>📧 Thông Tin Đăng Nhập</h3>
                <p><strong>Email/Tên đăng nhập:</strong> {username}</p>
                <p><strong>Mật khẩu tạm thời:</strong> <span style=""font-size: 18px; color: #e74c3c; font-weight: bold;"">{password}</span></p>
                <p><strong>URL đăng nhập:</strong> <a href=""https://aidefcom.io.vn"">https://aidefcom.io.vn</a></p>
                <p><strong>Vai trò:</strong> <span style=""color: #2196F3; font-weight: bold;"">Giảng viên</span></p>
            </div>
            
            <div class=""warning"">
                <h3>⚠️ Lưu Ý Quan Trọng</h3>
                <ul>
                    <li><strong>Đổi mật khẩu ngay</strong> sau lần đăng nhập đầu tiên để bảo mật tài khoản</li>
                    <li><strong>KHÔNG chia sẻ</strong> mật khẩu với bất kỳ ai, kể cả sinh viên</li>
                    <li>Mật khẩu trên <strong>CHỈ SỬ DỤNG MỘT LẦN</strong>, vui lòng đổi sang mật khẩu mạnh hơn</li>
                    <li>Nếu quên mật khẩu, sử dụng chức năng ""Quên mật khẩu"" trên trang đăng nhập</li>
                    <li>Quý Thầy/Cô có thể sử dụng Google Login với email này</li>
                </ul>
            </div>
            
            <p><strong>Hướng dẫn đăng nhập:</strong></p>
            <ol>
                <li>Truy cập <a href=""https://aidefcom.io.vn"">https://aidefcom.io.vn</a></li>
                <li>Nhập email: <strong>{username}</strong></li>
                <li>Nhập mật khẩu: <strong>{password}</strong></li>
                <li>Nhấn ""Đăng nhập"" (hoặc chọn ""Google Login"")</li>
                <li>Sau khi đăng nhập thành công, vào <strong>Cài đặt → Đổi mật khẩu</strong></li>
            </ol>
            
            <p><strong>Các tính năng dành cho Giảng viên:</strong></p>
            <ul>
                <li>📋 Quản lý hội đồng bảo vệ</li>
                <li>📊 Chấm điểm và đánh giá sinh viên</li>
                <li>📝 Xem transcript và phân tích AI</li>
                <li>📧 Gửi thông báo đến sinh viên</li>
            </ul>
            
            <a href=""https://aidefcom.io.vn/login"" class=""button"">🔐 Đăng Nhập Ngay</a>
        </div>
        <div class=""footer"">
            <p>Email này được gửi tự động từ hệ thống AIDefCom</p>
            <p>Nếu Quý Thầy/Cô có thắc mắc, vui lòng liên hệ quản trị viên hệ thống</p>
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
