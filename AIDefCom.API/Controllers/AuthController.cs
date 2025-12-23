using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.AppUser;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIDefCom.API.Controllers
{
    /// <summary>
    /// Controller for authentication and user management
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // ------------------ REGISTER ------------------

        /// <summary>
        /// Register a new user (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Id) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentNullException(nameof(request), "Id, Email, Password, and FullName are required");
            }

            _logger.LogInformation("Creating new account: {Email} with ID: {Id}", request.Email, request.Id);
            var user = await _authService.CreateAccountAsync(request);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = new
                {
                    user.Id,
                    user.Email,
                    user.EmailConfirmed,
                    user.FullName,
                    user.PhoneNumber
                }
            });
        }

        /// <summary>
        /// Register a new student user (Public)
        /// </summary>
        [HttpPost("register/student")]
        public async Task<IActionResult> RegisterWithRole([FromBody] AppUserDto request)
        {

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                throw new ArgumentNullException(nameof(request), "All fields (Email, Password, FullName, PhoneNumber) are required");
            }

            _logger.LogInformation("Registering new student user: {Email}", request.Email);
            var user = await _authService.RegisterWithRoleAsync(request);
            if (user == null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = new
                {
                    user.Id,
                    user.Email,
                    user.EmailConfirmed,
                    user.FullName,
                    user.PhoneNumber,
                    Role = "Student"
                }
            });
        }

        // ------------------ LOGIN ------------------
        
        /// <summary>
        /// User login (Public)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentNullException(nameof(request), "Email and password cannot be empty");
            }

            _logger.LogInformation("Login attempt for user: {Email}", request.Email);
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = result
            });
        }

        /// <summary>
        /// Google login (Public)
        /// </summary>
        [HttpPost("login/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            _logger.LogInformation("Google login attempt");
            var response = await _authService.GoogleLoginAsync(googleLoginDTO);

            if (!string.IsNullOrEmpty(response.TemporaryPassword))
            {
                return Ok(new ApiResponse<object>
                {
                    Code = ResponseCodes.Created,
                    Message = ResponseMessages.Created,
                    Data = new
                    {
                        note = "Below is your one-time password for normal login. Save it securely.",
                        temporaryPassword = response.TemporaryPassword,
                        tokenData = response
                    }
                });
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = response
            });
        }

        /// <summary>
        /// Google login as lecturer (Public)
        /// </summary>
        [HttpPost("login/google/lecturer")]
        public async Task<IActionResult> GoogleLoginAsLecturer([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            _logger.LogInformation("Google lecturer login attempt");
            var response = await _authService.GoogleLoginAsLecturerAsync(googleLoginDTO);

            if (!string.IsNullOrEmpty(response.TemporaryPassword))
            {
                return Ok(new ApiResponse<object>
                {
                    Code = ResponseCodes.Created,
                    Message = ResponseMessages.Created,
                    Data = new
                    {
                        note = "Below is your one-time password for normal login. Save it securely.",
                        temporaryPassword = response.TemporaryPassword,
                        tokenData = response
                    }
                });
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = response
            });
        }

        // ------------------ ROLE MANAGEMENT ------------------
        
        /// <summary>
        /// Assign a role to a user (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("roles/assign")]
        public async Task<IActionResult> AssignRole([FromBody] SetRoleRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Role))
            {
                throw new ArgumentNullException(nameof(request), "Email and role cannot be empty");
            }

            _logger.LogInformation("Assigning role {Role} to user {Email}", request.Role, request.Email);
            var result = await _authService.AssignRoleToUserAsync(request.Email, request.Role);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = new { Result = result }
            });
        }

        /// <summary>
        /// Create a new role (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("roles")]
        public async Task<IActionResult> AddRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName), "Role name cannot be empty");
            }

            _logger.LogInformation("Creating new role: {RoleName}", roleName);
            var result = await _authService.AddRoleAsync(roleName);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = ResponseMessages.Created,
                Data = new { Result = result }
            });
        }

        // ------------------ GOOGLE SET PASSWORD ------------------
        
        /// <summary>
        /// Set password for Google account (Authenticated)
        /// </summary>
        [Authorize]
        [HttpPost("google/set-password")]
        public async Task<IActionResult> GoogleSetPassword([FromBody] SetPasswordDTO setPasswordDTO, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new ArgumentException("Invalid authorization header");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            _logger.LogInformation("Setting password for Google account");
            var response = await _authService.GoogleSetPasswordAsync(setPasswordDTO, token);

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = response
            });
        }

        // ------------------ LOGOUT ------------------
        
        /// <summary>
        /// User logout (Authenticated)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("Email claim not found in token");
            }

            _logger.LogInformation("User logging out: {Email}", email);
            var result = await _authService.LogoutAsync(email);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = new { Result = result }
            });
        }

        // ------------------ CHANGE PASSWORD ------------------
        
        /// <summary>
        /// Change user password (Authenticated)
        /// </summary>
        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) ||
                string.IsNullOrEmpty(request.NewPassword) ||
                string.IsNullOrEmpty(request.ConfirmNewPassword))
            {
                throw new ArgumentNullException(nameof(request), "All password fields are required");
            }
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                throw new ArgumentException("New password and confirm password do not match");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("Email claim not found");
            }

            _logger.LogInformation("Changing password for user: {Email}", email);
            var result = await _authService.ChangePasswordAsync(email, request);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Updated,
                Data = new { Result = result }
            });
        }

        // ------------------ REFRESH TOKEN ------------------
        
        /// <summary>
        /// Refresh access token (Public)
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokens([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.UserId.ToString()) || string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new ArgumentNullException(nameof(request), "UserId and RefreshToken cannot be empty");
            }

            _logger.LogInformation("Refreshing token for user: {UserId}", request.UserId);
            var result = await _authService.RefreshTokensAsync(request);
            if (result == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = result
            });
        }

        // ------------------ ACCOUNT MANAGEMENT ------------------
        
        /// <summary>
        /// Hard delete a user account (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("accounts/{email}")]
        public async Task<IActionResult> DeleteAccount(string email)
        {
            _logger.LogInformation("Hard deleting account: {Email}", email);
            // Call hard delete on service. If not implemented, service should be updated accordingly.
            var result = await _authService.DeleteAccountAsync(email);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.NoContent,
                Message = string.Format(ResponseMessages.Deleted, "Account"),
                Data = new { Result = result }
            });
        }

        // ------------------ PASSWORD RECOVERY ------------------
        
        /// <summary>
        /// Request password reset (Public)
        /// </summary>
        [HttpPost("password/forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            _logger.LogInformation("Password reset requested for: {Email}", request.Email);
            var result = await _authService.ForgotPassword(request);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = new { Result = result }
            });
        }

        /// <summary>
        /// Reset password (Public)
        /// </summary>
        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            _logger.LogInformation("Resetting password for: {Email}", request.Email);
            var result = await _authService.ResetPassword(request);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                Data = new { Result = result }
            });
        }

        // ------------------ USER QUERIES ------------------
        
        /// <summary>
        /// Get all users (Admin and Moderator)
        /// </summary>
        [Authorize(Roles = "Admin,Moderator")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Retrieving all users");
            var users = await _authService.GetAllUsersAsync();
            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.ListRetrieved, "Users"),
                Data = users
            });
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("Retrieving user with ID: {Id}", id);
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "User"),
                Data = user
            });
        }

        /// <summary>
        /// Update user account information (Admin only)
        /// </summary>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] UpdateAccountDto request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentNullException(nameof(request), "FullName and Email are required");
            }
            if (!string.IsNullOrWhiteSpace(request.NewPassword) && string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
            {
                throw new ArgumentException("Confirm new password is required when changing password");
            }
            if (!string.IsNullOrWhiteSpace(request.NewPassword) && request.NewPassword != request.ConfirmNewPassword)
            {
                throw new ArgumentException("New password and confirm password do not match");
            }

            _logger.LogInformation("Admin updating account for user ID: {Id}", id);
            var updatedUser = await _authService.UpdateAccountAsync(id, request);
            return Ok(new ApiResponse<AppUserResponseDto>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Updated, "Account"),
                Data = updatedUser
            });
        }
    }
}
