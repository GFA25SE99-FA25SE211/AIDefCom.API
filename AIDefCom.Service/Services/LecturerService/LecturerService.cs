using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Import;
using AIDefCom.Service.Dto.Lecturer;
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

namespace AIDefCom.Service.Services.LecturerService
{
    public class LecturerService : ILecturerService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public LecturerService(IUnitOfWork uow, IMapper mapper, UserManager<AppUser> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                        if (DateTime.TryParse(dateOfBirthStr, out var dob))
                        {
                            dateOfBirth = dob;
                        }
                        else
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = row,
                                Field = "DateOfBirth",
                                ErrorMessage = "Invalid date format",
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

            worksheet.Cells[1, 1].Value = "UserName";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "FullName";
            worksheet.Cells[1, 4].Value = "DateOfBirth";
            worksheet.Cells[1, 5].Value = "Gender";
            worksheet.Cells[1, 6].Value = "PhoneNumber";
            worksheet.Cells[1, 7].Value = "Department";
            worksheet.Cells[1, 8].Value = "AcademicRank";
            worksheet.Cells[1, 9].Value = "Degree";

            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }

            worksheet.Cells[2, 1].Value = "lecturer001";
            worksheet.Cells[2, 2].Value = "lecturer001@university.edu.vn";
            worksheet.Cells[2, 3].Value = "Dr. Nguyen Van A";
            worksheet.Cells[2, 4].Value = "01/15/1980";
            worksheet.Cells[2, 5].Value = "Male";
            worksheet.Cells[2, 6].Value = "0123456789";
            worksheet.Cells[2, 7].Value = "Computer Science";
            worksheet.Cells[2, 8].Value = "Associate Professor";
            worksheet.Cells[2, 9].Value = "PhD";

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
