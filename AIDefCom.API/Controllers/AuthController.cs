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
        /// Register a new user
        /// </summary>
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { Message = "All fields (Email, Password, FullName, PhoneNumber) are required" });
            }

            _logger.LogInformation("Creating new account: {Email}", request.Email);

            try
            {
                var user = await _authService.CreateAccountAsync(request);

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.EmailConfirmed,
                    user.FullName,
                    user.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Account creation failed for email: {Email}", request.Email);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Register a new student user
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
                _logger.LogWarning("Student registration failed for email: {Email}", request.Email);
                throw new InvalidOperationException("User with this email already exists");
            }

            _logger.LogInformation("Student registered successfully: {Email}", request.Email);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Created,
                Message = "User registered successfully with role 'Student'",
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
        /// User login
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
                _logger.LogWarning("Login failed for user: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Login successful",
                Data = result
            });
        }

        /// <summary>
        /// Google login
        /// </summary>
        [HttpPost("login/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            _logger.LogInformation("Google login attempt");
            var response = await _authService.GoogleLoginAsync(googleLoginDTO);

            if (!string.IsNullOrEmpty(response.TemporaryPassword))
            {
                _logger.LogInformation("New Google account created with temporary password");
                return Ok(new ApiResponse<object>
                {
                    Code = ResponseCodes.Created,
                    Message = "Account created successfully with Google login",
                    Data = new
                    {
                        note = "Below is your one-time password for normal login. Save it securely.",
                        temporaryPassword = response.TemporaryPassword,
                        tokenData = response
                    }
                });
            }

            _logger.LogInformation("Google login successful");
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Google login successful",
                Data = response
            });
        }

        /// <summary>
        /// Google login as member
        /// </summary>
        [HttpPost("login/google/member")]
        public async Task<IActionResult> GoogleLoginAsMember([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            _logger.LogInformation("Google member login attempt");
            var response = await _authService.GoogleLoginAsMemberAsync(googleLoginDTO);

            if (!string.IsNullOrEmpty(response.TemporaryPassword))
            {
                _logger.LogInformation("New Google member account created with temporary password");
                return Ok(new ApiResponse<object>
                {
                    Code = ResponseCodes.Created,
                    Message = "Account created successfully with Google login",
                    Data = new
                    {
                        note = "Below is your one-time password for normal login. Save it securely.",
                        temporaryPassword = response.TemporaryPassword,
                        tokenData = response
                    }
                });
            }

            _logger.LogInformation("Google member login successful");
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Google member login successful",
                Data = response
            });
        }

        // ------------------ ROLE MANAGEMENT ------------------
        
        /// <summary>
        /// Assign a role to a user
        /// </summary>
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
                Message = result
            });
        }

        /// <summary>
        /// Create a new role
        /// </summary>
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
                Message = result
            });
        }

        // ------------------ GOOGLE SET PASSWORD ------------------
        
        /// <summary>
        /// Set password for Google account
        /// </summary>
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
                Message = "Password set successfully",
                Data = response
            });
        }

        // ------------------ LOGOUT ------------------
        
        /// <summary>
        /// User logout
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
                Message = result
            });
        }

        // ------------------ CHANGE PASSWORD ------------------
        
        /// <summary>
        /// Change user password
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
                Message = result
            });
        }

        // ------------------ REFRESH TOKEN ------------------
        
        /// <summary>
        /// Refresh access token
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
                _logger.LogWarning("Invalid refresh token for user: {UserId}", request.UserId);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = "Token refreshed successfully",
                Data = result
            });
        }

        // ------------------ ACCOUNT MANAGEMENT ------------------
        
        /// <summary>
        /// Soft delete a user account
        /// </summary>
        [HttpDelete("accounts/{email}")]
        public async Task<IActionResult> SoftDeleteAccount(string email)
        {
            _logger.LogInformation("Soft deleting account: {Email}", email);
            var result = await _authService.SoftDeleteAccountAsync(email);
            
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = result
            });
        }

        /// <summary>
        /// Restore a soft-deleted account
        /// </summary>
        [HttpPut("accounts/{email}/restore")]
        public async Task<IActionResult> RestoreAccount(string email)
        {
            _logger.LogInformation("Restoring account: {Email}", email);
            var result = await _authService.RestoreAccountAsync(email);
            
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = result
            });
        }

        // ------------------ PASSWORD RECOVERY ------------------
        
        /// <summary>
        /// Request password reset
        /// </summary>
        [HttpPost("password/forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            _logger.LogInformation("Password reset requested for: {Email}", request.Email);
            var result = await _authService.ForgotPassword(request);
            
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = result
            });
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            _logger.LogInformation("Resetting password for: {Email}", request.Email);
            var result = await _authService.ResetPassword(request);
            
            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = result
            });
        }

        // ------------------ USER QUERIES ------------------
        
        /// <summary>
        /// Get all users
        /// </summary>
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
        /// Get user by ID
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("Retrieving user with ID: {Id}", id);
            var user = await _authService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found", id);
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            return Ok(new ApiResponse<object>
            {
                Code = ResponseCodes.Success,
                Message = string.Format(ResponseMessages.Retrieved, "User"),
                Data = user
            });
        }
    }
}
