using AIDefCom.Repository.Entities;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.AppUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.AuthService
{
    public interface IAuthService
    {
        Task<AppUser?> RegisterAsync(AppUserDto request);

        Task<TokenResponseDto?> LoginAsync(LoginDto request);

        Task<string> LogoutAsync(string username);

        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);

        Task<string> AssignRoleToUserAsync(string username, string role);

        Task<string> AddRoleAsync(string roleName);

        Task<string> ChangePasswordAsync(string username, ChangePasswordDto request);

        Task<string> ForgotPassword(ForgotPasswordDto request);

        Task<string> ResetPassword(ResetPasswordDto request);

        Task<TokenResponseDto> GoogleLoginAsync(GoogleUserLoginDTO googleLoginDTO);

        Task<TokenResponseDto> GoogleLoginAsMemberAsync(GoogleUserLoginDTO googleLoginDTO);

        Task<TokenResponseDto> GoogleSetPasswordAsync(SetPasswordDTO request, string token);

        Task<string> SoftDeleteAccountAsync(string username);

        Task<string> RestoreAccountAsync(string username);

        Task<AppUser?> RegisterWithRoleAsync(AppUserDto request);
    }
}
