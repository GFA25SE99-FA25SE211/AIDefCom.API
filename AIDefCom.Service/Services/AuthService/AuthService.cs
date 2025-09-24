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
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.AuthService
{
    public class AuthService(UserManager<AppUser> _userManager, SignInManager<AppUser> _signInManager, RoleManager<IdentityRole> _roleManager, IConfiguration _configuration, IEmailService _emailService) : IAuthService
    {
        public async Task<AppUser?> RegisterAsync(AppUserDto request)
        {
            if (await _userManager.FindByNameAsync(request.Username) != null)
                throw new Exception("Username already exists.");

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new Exception("Email already exists.");

            var user = new AppUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = false,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception($"Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return user;
        }
        public async Task<AppUser?> RegisterWithRoleAsync(AppUserDto request)
        {
            if (await _userManager.FindByNameAsync(request.Username) != null)
                throw new Exception("Username already exists.");

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new Exception("Email already exists.");

            var user = new AppUser
            {
                UserName = request.Username,
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


        public async Task<string> AddRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
                throw new Exception("Role already exists.");

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new Exception($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return "Role created successfully.";
        }

        public async Task<string> AssignRoleToUserAsync(string username, string role)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception("User not found.");

            if (!await _roleManager.RoleExistsAsync(role))
                throw new Exception("Role does not exist.");

            await _userManager.AddToRoleAsync(user, role);
            return "Role assigned successfully.";
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                throw new Exception("Invalid username or password.");

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
                throw new Exception("Invalid username or password.");

            var tokenResponse = await CreateTokenResponse(user);
            tokenResponse.EmailConfirmed = user.EmailConfirmed;

            return tokenResponse;
        }

        public async Task<string> LogoutAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception("User not found.");

            await _signInManager.SignOutAsync();

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return "Logout successful.";
        }

        public async Task<string> ChangePasswordAsync(string username, ChangePasswordDto request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
                throw new Exception("New password and confirm password do not match.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                throw new Exception($"Password change failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return "Password changed successfully.";
        }
        public async Task<string> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}/reset.html?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";

            var message = new MessageOTP(
                new string[] { request.Email },
                "Password Reset Request",
                $@"
        <h1>Password Reset</h1>
        <p>Dear {request.Email},</p>
        <p>Click the link below to reset your password:</p>
        <p><a href='{resetLink}'>Reset Password</a></p>
        <p>The link is valid for a limited time.</p>"
            );

            _emailService.SendEmail(message);
            return "Password reset email sent.";
        }


        public async Task<string> ResetPassword(ResetPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Password reset failed: {errors}");
            }

            return "Password has been reset successfully.";
        }

        public async Task<string> SoftDeleteAccountAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception("User not found.");

            user.IsDelete = true;
            await _userManager.UpdateAsync(user);

            return "User account has been soft deleted.";
        }

        public async Task<string> RestoreAccountAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception("User not found.");

            if (!user.IsDelete)
                throw new Exception("User account is already active.");

            user.IsDelete = false;
            await _userManager.UpdateAsync(user);

            return "User account has been restored.";
        }


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
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

        public async Task<TokenResponseDto> GoogleLoginAsync(GoogleUserLoginDTO googleLoginDTO)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDTO.Token, new GoogleJsonWebSignature.ValidationSettings());
            if (payload == null)
                throw new Exception("Invalid Id Token");

            var user = await _userManager.FindByEmailAsync(payload.Email);
            bool isNewUser = false;

            if (user == null)
            {
                isNewUser = true;
                user = new AppUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    throw new Exception("Failed to create user");

                await _userManager.AddToRoleAsync(user, "User");

                var info = new UserLoginInfo("Google", payload.Subject, "Google");
                var loginResult = await _userManager.AddLoginAsync(user, info);
                if (!loginResult.Succeeded)
                    throw new Exception("Failed to add external login");
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            var refreshToken = await GenerateAndSaveRefreshToken(user);

            return new TokenResponseDto
            {
                EmailConfirmed = user.EmailConfirmed,
                AccessToken = CreateToken(user),
                RefreshToken = refreshToken,
                HasPassword = hasPassword,
                IsNewUser = isNewUser
            };
        }

        public async Task<TokenResponseDto> GoogleSetPasswordAsync(SetPasswordDTO request, string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                throw new Exception("Invalid token format");

            var jwtToken = handler.ReadJwtToken(token);
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

    }
}
