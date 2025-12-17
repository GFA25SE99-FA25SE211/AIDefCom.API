using AIDefCom.Repository.Entities;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.AppUser;
using AIDefCom.Service.Services.EmailService;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.AuthService
{
    public class AuthService(
        UserManager<AppUser> _userManager,
        SignInManager<AppUser> _signInManager,
        RoleManager<IdentityRole> _roleManager,
        IConfiguration _configuration,
        IEmailService _emailService
    ) : IAuthService
    {
        // ------------------ REGISTER ------------------
        public async Task<AppUser?> CreateAccountAsync(CreateAccountDto request)
        {
            // Validate ID format
            var (isIdValid, idError) = ValidateUserId(request.Id);
            if (!isIdValid)
                throw new ArgumentException(idError, nameof(request.Id));

            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(request.Email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(request.Email));

            // Validate full name
            var (isNameValid, nameError) = ValidateFullName(request.FullName);
            if (!isNameValid)
                throw new ArgumentException(nameError, nameof(request.FullName));

            // Validate phone number
            var (isPhoneValid, phoneError) = ValidatePhoneNumber(request.PhoneNumber);
            if (!isPhoneValid)
                throw new ArgumentException(phoneError, nameof(request.PhoneNumber));

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new Exception("Email already exists.");

            // Kiểm tra xem ID đã tồn tại chưa
            if (await _userManager.FindByIdAsync(request.Id) != null)
                throw new Exception("ID already exists.");

            // Validate password
            var (isValid, errorMessage) = ValidatePassword(request.Password);
            if (!isValid)
                throw new Exception(errorMessage);

            var user = new AppUser
            {
                Id = request.Id,
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
                IsDelete = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception($"Create account failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return user;
        }

        public async Task<AppUser?> RegisterWithRoleAsync(AppUserDto request)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(request.Email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(request.Email));

            // Validate full name
            var (isNameValid, nameError) = ValidateFullName(request.FullName);
            if (!isNameValid)
                throw new ArgumentException(nameError, nameof(request.FullName));

            // Validate phone number
            var (isPhoneValid, phoneError) = ValidatePhoneNumber(request.PhoneNumber);
            if (!isPhoneValid)
                throw new ArgumentException(phoneError, nameof(request.PhoneNumber));

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new Exception("Email already exists.");

            // Validate password
            var (isValid, errorMessage) = ValidatePassword(request.Password);
            if (!isValid)
                throw new Exception(errorMessage);

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = false,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception($"Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            var roleExists = await _roleManager.RoleExistsAsync("Student");
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Student"));
                if (!roleResult.Succeeded)
                    throw new Exception("Failed to create Student role.");
            }

            await _userManager.AddToRoleAsync(user, "Student");
            return user;
        }

        // ------------------ ROLE MANAGEMENT ------------------
        public async Task<string> AddRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name is required", nameof(roleName));

            // Validate role name format (chỉ chứa chữ cái)
            if (!Regex.IsMatch(roleName, @"^[a-zA-Z]+$"))
                throw new ArgumentException("Role name can only contain letters", nameof(roleName));

            if (roleName.Length < 3 || roleName.Length > 50)
                throw new ArgumentException("Role name must be between 3 and 50 characters", nameof(roleName));

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
                throw new Exception("Role already exists.");

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new Exception($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return "Role created successfully.";
        }

        public async Task<string> AssignRoleToUserAsync(string email, string role)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(email));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role name is required", nameof(role));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            if (!await _roleManager.RoleExistsAsync(role))
                throw new Exception("Role does not exist.");

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    throw new Exception("Failed to remove existing roles.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
                throw new Exception("Failed to assign the new role.");

            return "Role assigned successfully.";
        }


        // ------------------ LOGIN / LOGOUT ------------------
        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(request.Email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(request.Email));

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required", nameof(request.Password));

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
                throw new Exception("Invalid email or password.");

            var tokenResponse = await CreateTokenResponse(user);
            tokenResponse.EmailConfirmed = user.EmailConfirmed;

            return tokenResponse;
        }

        public async Task<string> LogoutAsync(string email)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            await _signInManager.SignOutAsync();

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return "Logout successful.";
        }

        // ------------------ PASSWORD MANAGEMENT ------------------
        public async Task<string> ChangePasswordAsync(string email, ChangePasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                throw new ArgumentException("Current password is required", nameof(request.CurrentPassword));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("New password is required", nameof(request.ConfirmNewPassword));

            if (string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
                throw new ArgumentException("Confirm new password is required", nameof(request.ConfirmNewPassword));

            if (request.NewPassword != request.ConfirmNewPassword)
                throw new Exception("New password and confirm password do not match.");

            // Validate new password
            var (isValid, errorMessage) = ValidatePassword(request.NewPassword);
            if (!isValid)
                throw new Exception(errorMessage);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                throw new Exception($"Password change failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return "Password changed successfully.";
        }

        public async Task<string> ForgotPassword(ForgotPasswordDto request)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(request.Email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(request.Email));

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";

            var message = new MessageOTP(
                new string[] { request.Email },
                "🔐 AIDefCom - Yêu Cầu Đặt Lại Mật Khẩu",
                $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Request</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f7fa; line-height: 1.6;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f4f7fa; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);"">
                    
                    <!-- Header Section -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); padding: 40px 30px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;"">
                                🔐 AIDefCom
                            </h1>
                            <p style=""margin: 10px 0 0 0; color: #fef3c7; font-size: 15px; font-weight: 400;"">
                                Yêu Cầu Đặt Lại Mật Khẩu
                            </p>
                        </td>
                    </tr>

                    <!-- Main Content -->
                    <tr>
                        <td style=""padding: 40px 30px 30px 30px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #1f2937; font-size: 24px; font-weight: 600;"">
                                Xin chào, {user.FullName}! 👋
                            </h2>
                            <p style=""margin: 0 0 20px 0; color: #6b7280; font-size: 16px; line-height: 1.6;"">
                                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Nếu đây không phải là yêu cầu của bạn, vui lòng bỏ qua email này.
                            </p>
                        </td>
                    </tr>

                    <!-- Reset Info Card -->
                    <tr>
                        <td style=""padding: 0 30px 30px 30px;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); border-radius: 8px; border-left: 4px solid #f59e0b; overflow: hidden;"">
                                <tr>
                                    <td style=""padding: 25px;"">
                                        <h3 style=""margin: 0 0 15px 0; color: #92400e; font-size: 18px; font-weight: 600;"">
                                            ⏰ Thông Tin Quan Trọng
                                        </h3>
                                        <ul style=""margin: 0; padding-left: 20px; color: #78350f; font-size: 14px; line-height: 1.8;"">
                                            <li style=""margin-bottom: 8px;"">
                                                Link đặt lại mật khẩu chỉ <strong>có hiệu lực trong 1 giờ</strong>
                                            </li>
                                            <li style=""margin-bottom: 8px;"">
                                                Link chỉ có thể <strong>sử dụng một lần duy nhất</strong>
                                            </li>
                                            <li>
                                                Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng <strong>bỏ qua email này</strong>
                                            </li>
                                        </ul>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Call to Action Button -->
                    <tr>
                        <td style=""padding: 0 30px 30px 30px; text-align: center;"">
                            <a href=""{resetLink}"" style=""display: inline-block; padding: 16px 40px; background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); color: #ffffff; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: 600; box-shadow: 0 4px 6px rgba(245, 158, 11, 0.3); transition: all 0.3s ease;"">
                                🔓 Đặt Lại Mật Khẩu Ngay
                            </a>
                            <p style=""margin: 15px 0 0 0; color: #9ca3af; font-size: 13px; line-height: 1.5;"">
                            </p>
                        </td>
                    </tr>

                    <!-- Security Tips -->
                    <tr>
                        <td style=""padding: 0 30px 30px 30px;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #fef2f2; border-radius: 8px; border-left: 4px solid #dc2626;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <h3 style=""margin: 0 0 15px 0; color: #991b1b; font-size: 16px; font-weight: 600;"">
                                            🛡️ Lưu Ý Bảo Mật
                                        </h3>
                                        <ul style=""margin: 0; padding-left: 20px; color: #7f1d1d; font-size: 14px; line-height: 1.8;"">
                                            <li style=""margin-bottom: 8px;"">
                                                <strong>KHÔNG chia sẻ</strong> link này với bất kỳ ai
                                            </li>
                                            <li style=""margin-bottom: 8px;"">
                                                AIDefCom <strong>không bao giờ</strong> yêu cầu mật khẩu qua email
                                            </li>
                                            <li style=""margin-bottom: 8px;"">
                                                Mật khẩu mới phải có <strong>8-16 ký tự</strong>
                                            </li>
                                            <li style=""margin-bottom: 8px;"">
                                                Bao gồm <strong>ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt</strong>
                                            </li>
                                            <li>
                                                Nếu bạn nghi ngờ email lừa đảo, vui lòng liên hệ quản trị viên
                                            </li>
                                        </ul>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Help Section -->
                    <tr>
                        <td style=""padding: 0 30px 30px 30px;"">
                            <h3 style=""margin: 0 0 20px 0; color: #1f2937; font-size: 18px; font-weight: 600;"">
                                🤔 Gặp Vấn Đề?
                            </h3>
                            <p style=""margin: 0 0 15px 0; color: #6b7280; font-size: 15px; line-height: 1.6;"">
                                Nếu bạn không thực hiện yêu cầu này hoặc gặp vấn đề khi đặt lại mật khẩu:
                            </p>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f9fafb; border-radius: 8px; padding: 20px;"">
                                <tr>
                                    <td style=""text-align: center;"">
                                        <p style=""margin: 0 0 10px 0; color: #6b7280; font-size: 14px;"">
                                            📧 Email hỗ trợ: <a href=""mailto:support@aidefcom.io.vn"" style=""color: #f59e0b; text-decoration: none; font-weight: 600;"">support@aidefcom.io.vn</a>
                                        </p>
                                        <p style=""margin: 0; color: #6b7280; font-size: 14px;"">
                                            ⏰ Thời gian hỗ trợ: Thứ 2 - Thứ 6 (8:00 - 17:00)
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Additional Information -->
                    <tr>
                        <td style=""padding: 0 30px 40px 30px;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""border-top: 1px solid #e5e7eb; padding-top: 20px;"">
                                <tr>
                                    <td style=""text-align: center;"">
                                        <p style=""margin: 0 0 10px 0; color: #6b7280; font-size: 13px; line-height: 1.5;"">
                                            <strong>Email gửi đến:</strong> {user.Email}<br>
                                            <strong>Thời gian yêu cầu:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #1f2937; padding: 30px; text-align: center;"">
                            <p style=""margin: 0 0 10px 0; color: #9ca3af; font-size: 13px; line-height: 1.6;"">
                                Email này được gửi tự động từ hệ thống AIDefCom<br>
                                Vui lòng không trả lời email này
                            </p>
                            <p style=""margin: 10px 0 0 0; color: #6b7280; font-size: 12px;"">
                                &copy; {DateTime.Now.Year} AIDefCom. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
"
            );

            _emailService.SendEmail(message);
            return $"Password reset email sent to {request.Email}.";
        }



        public async Task<string> ResetPassword(ResetPasswordDto request)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(request.Email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(request.Email));

            if (string.IsNullOrWhiteSpace(request.Token))
                throw new ArgumentException("Reset token is required", nameof(request.Token));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("New password is required", nameof(request.NewPassword));

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("User not found.");

            // Validate new password
            var (isValid, errorMessage) = ValidatePassword(request.NewPassword);
            if (!isValid)
                throw new Exception(errorMessage);

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
                throw new Exception($"Password reset failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return "Password has been reset successfully.";
        }


        // ------------------ ACCOUNT STATUS ------------------
        public async Task<string> SoftDeleteAccountAsync(string email)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            user.IsDelete = true;
            await _userManager.UpdateAsync(user);

            return "User account has been soft deleted.";
        }

        public async Task<string> RestoreAccountAsync(string email)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            if (!user.IsDelete)
                throw new Exception("User account is already active.");

            user.IsDelete = false;
            await _userManager.UpdateAsync(user);

            return "User account has been restored.";
        }

        public async Task<string> DeleteAccountAsync(string email)
        {
            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new Exception($"Hard delete failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return "User account has been hard deleted.";
        }

        // ------------------ TOKEN MANAGEMENT ------------------
        private async Task<TokenResponseDto> CreateTokenResponse(AppUser user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user),
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            if (request.UserId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(request.UserId));

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("Refresh token is required", nameof(request.RefreshToken));

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new Exception("Invalid refresh token.");

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new Exception("Refresh token expired or invalid.");

            return await CreateTokenResponse(user);
        }

        private async Task<string> GenerateAndSaveRefreshToken(AppUser user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            return refreshToken;
        }

        private string CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("EmailConfirmed", user.EmailConfirmed.ToString())
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var secretKey = _configuration["AppSettings:Token"];
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("JWT Secret Key is missing in appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateSecurePassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghjkmnopqrstuvwxyz0123456789!@#$%^&*?";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ------------------ VALIDATION METHODS ------------------

        /// <summary>
        /// Validate email format theo chuẩn RFC 5322
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required");

            if (email.Length > 256)
                return (false, "Email must not exceed 256 characters");

            // Regex pattern chuẩn RFC 5322
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
                return (false, "Invalid email format");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate phone number format (VN: +84xxxxxxxxx hoặc 0xxxxxxxxx)
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidatePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return (true, string.Empty); // Phone number là optional

            // Cho phép format: +84xxxxxxxxx hoặc 0xxxxxxxxx (VN)
            var phonePattern = @"^(\+84|0)[0-9]{9,10}$";
            if (!Regex.IsMatch(phoneNumber, phonePattern))
                return (false, "Invalid phone number format. Must be +84xxxxxxxxx or 0xxxxxxxxx");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate full name (chỉ chữ cái và khoảng trắng, 2-100 ký tự)
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Full name is required");

            if (fullName.Length < 2)
                return (false, "Full name must be at least 2 characters long");

            if (fullName.Length > 100)
                return (false, "Full name must not exceed 100 characters");

            // Chỉ cho phép chữ cái, khoảng trắng, dấu tiếng Việt
            var namePattern = @"^[\p{L}\s]+$";
            if (!Regex.IsMatch(fullName, namePattern))
                return (false, "Full name can only contain letters and spaces");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate User ID format (SE123456, GV123456, LT123456)
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "User ID is required");

            // Format: SE123456 (Student), GV123456 (Lecturer), LT123456
            var idPattern = @"^(SE|GV|LT)[0-9]{6}$";
            if (!Regex.IsMatch(userId, idPattern))
                return (false, "Invalid ID format. Must be SE123456 (Student), GV123456 (Lecturer), or LT123456");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate password theo yêu cầu:
        /// - Độ dài: 8-16 ký tự
        /// - Ít nhất 1 chữ hoa
        /// - Ít nhất 1 chữ thường
        /// - Ít nhất 1 số
        /// - Ít nhất 1 ký tự đặc biệt
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long");

            if (password.Length > 16)
                return (false, "Password must not exceed 16 characters");

            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter");

            if (!password.Any(char.IsLower))
                return (false, "Password must contain at least one lowercase letter");

            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one number");

            // Ký tự đặc biệt
            var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            if (!password.Any(c => specialChars.Contains(c)))
                return (false, "Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)");

            return (true, string.Empty);
        }

        // ------------------ GOOGLE AUTH ------------------
        public async Task<TokenResponseDto> GoogleLoginAsync(GoogleUserLoginDTO googleLoginDTO)
        {
            if (string.IsNullOrWhiteSpace(googleLoginDTO.Token))
                throw new ArgumentException("Google token is required", nameof(googleLoginDTO.Token));

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                googleLoginDTO.Token,
                new GoogleJsonWebSignature.ValidationSettings()
            );

            if (payload == null)
                throw new Exception("Invalid Google ID token.");

            var user = await _userManager.FindByEmailAsync(payload.Email);
            string? firstGeneratedPassword = null;

            // Nếu user không tồn tại trong hệ thống, không cho phép login
            if (user == null)
            {
                // Return 404 instead of 500 when email not registered
                throw new KeyNotFoundException("Email is not registered in the system. Please contact administrator to create an account.");
            }

            // Kiểm tra nếu tài khoản bị soft delete
            if (user.IsDelete)
            {
                throw new Exception("This account has been deactivated. Please contact administrator.");
            }

            // Cập nhật FullName từ Google nếu chưa có
            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(payload.Name))
            {
                user.FullName = payload.Name;
                await _userManager.UpdateAsync(user);
            }

            // Nếu user chưa có password, tạo password tự động cho việc login thông thường
            bool hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                string randomPassword = GenerateSecurePassword();
                var addPasswordResult = await _userManager.AddPasswordAsync(user, randomPassword);

                if (addPasswordResult.Succeeded)
                {
                    user.HasGeneratedPassword = true;
                    user.LastGeneratedPassword = randomPassword;
                    user.PasswordGeneratedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    firstGeneratedPassword = randomPassword;
                }
            }

            // Đảm bảo user có role Student
            var defaultRole = "Student";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
                await _roleManager.CreateAsync(new IdentityRole(defaultRole));
            if (!await _userManager.IsInRoleAsync(user, defaultRole))
                await _userManager.AddToRoleAsync(user, defaultRole);

            // Thêm external login nếu chưa có
            var info = new UserLoginInfo("Google", payload.Subject, "Google");
            if (await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) == null)
                await _userManager.AddLoginAsync(user, info);

            // Tạo token và refresh token
            var refreshToken = await GenerateAndSaveRefreshToken(user);
            var accessToken = CreateToken(user);

            var response = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                EmailConfirmed = user.EmailConfirmed,
                HasPassword = true,
                IsNewUser = false,
                TemporaryPassword = firstGeneratedPassword
            };

            // Xóa temporary password sau khi trả về cho user
            if (firstGeneratedPassword != null)
            {
                user.LastGeneratedPassword = null;
                await _userManager.UpdateAsync(user);
            }

            return response;
        }

        public async Task<TokenResponseDto> GoogleLoginAsLecturerAsync(GoogleUserLoginDTO googleLoginDTO)
        {
            if (string.IsNullOrWhiteSpace(googleLoginDTO.Token))
                throw new ArgumentException("Google token is required", nameof(googleLoginDTO.Token));

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                googleLoginDTO.Token,
                new GoogleJsonWebSignature.ValidationSettings()
            );

            if (payload == null)
                throw new Exception("Invalid Google ID token.");

            var user = await _userManager.FindByEmailAsync(payload.Email);
            string? firstGeneratedPassword = null;

            if (user == null)
            {
                // Return 404 instead of 500 when email not registered for lecturer login
                throw new KeyNotFoundException("Email is not registered in the system. Please contact administrator to create an account.");
            }

            if (user.IsDelete)
            {
                throw new Exception("This account has been deactivated. Please contact administrator.");
            }

            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(payload.Name))
            {
                user.FullName = payload.Name;
                await _userManager.UpdateAsync(user);
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                string randomPassword = GenerateSecurePassword();
                var addPasswordResult = await _userManager.AddPasswordAsync(user, randomPassword);

                if (addPasswordResult.Succeeded)
                {
                    user.HasGeneratedPassword = true;
                    user.LastGeneratedPassword = randomPassword;
                    user.PasswordGeneratedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    firstGeneratedPassword = randomPassword;
                }
            }

            // Đảm bảo user có role Lecturer
            var defaultRole = "Lecturer";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
                await _roleManager.CreateAsync(new IdentityRole(defaultRole));
            if (!await _userManager.IsInRoleAsync(user, defaultRole))
                await _userManager.AddToRoleAsync(user, defaultRole);

            // Thêm external login nếu chưa có
            var info = new UserLoginInfo("Google", payload.Subject, "Google");
            if (await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) == null)
                await _userManager.AddLoginAsync(user, info);

            // Tạo token và refresh token
            var refreshToken = await GenerateAndSaveRefreshToken(user);
            var accessToken = CreateToken(user);

            var response = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                EmailConfirmed = user.EmailConfirmed,
                HasPassword = true,
                IsNewUser = false,
                TemporaryPassword = firstGeneratedPassword
            };

            // Xóa temporary password sau khi trả về cho user
            if (firstGeneratedPassword != null)
            {
                user.LastGeneratedPassword = null;
                await _userManager.UpdateAsync(user);
            }

            return response;
        }

        public async Task<TokenResponseDto> GoogleSetPasswordAsync(SetPasswordDTO request, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required", nameof(token));

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required", nameof(request.Password));

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
                throw new ArgumentException("Confirm password is required", nameof(request.ConfirmPassword));

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                throw new Exception("Invalid token format");

            var jwtToken = handler.ReadJwtToken(token);

            // Kiểm tra token đã hết hạn chưa
            if (jwtToken.ValidTo < DateTime.UtcNow)
                throw new Exception("Token has expired");

            var email = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                throw new Exception("Email claim not found in token");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found");

            if (await _userManager.HasPasswordAsync(user))
                throw new Exception("Password already set");

            if (request.Password != request.ConfirmPassword)
                throw new Exception("Passwords do not match");

            // Validate password
            var (isValid, errorMessage) = ValidatePassword(request.Password);
            if (!isValid)
                throw new Exception(errorMessage);

            var result = await _userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception("Failed to set password");

            var refreshToken = await GenerateAndSaveRefreshToken(user);

            return new TokenResponseDto
            {
                EmailConfirmed = user.EmailConfirmed,
                AccessToken = CreateToken(user),
                RefreshToken = refreshToken,
                HasPassword = true
            };
        }

        public async Task<IEnumerable<AppUserListDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<AppUserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var mainRole = roles.FirstOrDefault() ?? "No Role";

                result.Add(new AppUserListDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = mainRole,
                    IsDelete = user.IsDelete,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return result;
        }

        public async Task<AppUserResponseDto?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return new AppUserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsDelete = user.IsDelete,
                Roles = roles,
                HasGeneratedPassword = user.HasGeneratedPassword,
                PasswordGeneratedAt = user.PasswordGeneratedAt,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
            };
        }

        // ------------------ UPDATE ACCOUNT ------------------
        public async Task<AppUserResponseDto> UpdateAccountAsync(string userId, UpdateAccountDto request)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            // Validate email format
            var (isEmailValid, emailError) = ValidateEmail(request.Email);
            if (!isEmailValid)
                throw new ArgumentException(emailError, nameof(request.Email));

            // Validate full name
            var (isNameValid, nameError) = ValidateFullName(request.FullName);
            if (!isNameValid)
                throw new ArgumentException(nameError, nameof(request.FullName));

            // Validate phone number
            var (isPhoneValid, phoneError) = ValidatePhoneNumber(request.PhoneNumber);
            if (!isPhoneValid)
                throw new ArgumentException(phoneError, nameof(request.PhoneNumber));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            // Kiểm tra email mới có trùng với user khác không
            if (user.Email != request.Email)
            {
                var existingUserWithEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
                    throw new Exception("Email already exists.");
                
                user.Email = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.UserName = request.Email;
                user.NormalizedUserName = request.Email.ToUpper();
            }

            // Cập nhật thông tin
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;

            // Xử lý reset password (Admin có quyền reset password trực tiếp)
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                // Kiểm tra NewPassword và ConfirmNewPassword khớp nhau
                if (string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
                    throw new Exception("Confirm new password is required.");

                if (request.NewPassword != request.ConfirmNewPassword)
                    throw new Exception("New password and confirm password do not match.");

                // Validate password
                var (isValid, errorMessage) = ValidatePassword(request.NewPassword);
                if (!isValid)
                    throw new Exception(errorMessage);

                // Admin reset password: Xóa password cũ và tạo password mới
                if (await _userManager.HasPasswordAsync(user))
                {
                    // Xóa password cũ
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (!removePasswordResult.Succeeded)
                    {
                        var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to remove old password: {errors}");
                    }
                }

                // Thêm password mới
                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to set new password: {errors}");
                }

                // Cập nhật flag password generated
                user.HasGeneratedPassword = false;
                user.PasswordGeneratedAt = DateTime.UtcNow;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception($"Update account failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Trả về thông tin user đã cập nhật
            var roles = await _userManager.GetRolesAsync(user);
            return new AppUserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsDelete = user.IsDelete,
                Roles = roles,
                HasGeneratedPassword = user.HasGeneratedPassword,
                PasswordGeneratedAt = user.PasswordGeneratedAt,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
            };
        }
    }
}
