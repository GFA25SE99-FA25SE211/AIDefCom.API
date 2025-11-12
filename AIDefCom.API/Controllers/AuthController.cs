using AIDefCom.Service.Dto.Account;
using AIDefCom.Service.Dto.AppUser;
using AIDefCom.Service.Dto.Common;
using AIDefCom.Service.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ------------------ REGISTER ------------------
        
        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AppUserDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Message = "All fields (Email, Password, FullName, PhoneNumber) are required."
                });
            }

            var user = await _authService.RegisterAsync(request);
            if (user == null)
                return Conflict(new ApiResponse<object>
                {
                    Message = "Registration failed. User may already exist."
                });

            return Ok(new ApiResponse<object>
            {
                Message = "Register successfully!",
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "All fields (Email, Password, FullName, PhoneNumber) are required."
                });
            }

            var user = await _authService.RegisterWithRoleAsync(request);
            if (user == null)
                return Conflict(new ApiResponse<object>
                {
                    Message = "Registration failed. User may already exist."
                });

            return Ok(new ApiResponse<object>
            {
                Message = "User registered successfully with role 'Student'.",
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Email and password cannot be empty."
                });

            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new ApiResponse<object>
                {
                    Message = "Invalid email or password."
                });

            return Ok(new ApiResponse<object>
            {
                Message = "Login successful.",
                Data = result
            });
        }

        /// <summary>
        /// Google login
        /// </summary>
        [HttpPost("login/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            var response = await _authService.GoogleLoginAsync(googleLoginDTO);

            if (!string.IsNullOrEmpty(response.TemporaryPassword))
            {
                return Ok(new ApiResponse<object>
                {
                    Message = "Account created successfully with Google login.",
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
                Message = "Google login successful.",
                Data = response
            });
        }

        /// <summary>
        /// Google login as member
        /// </summary>
        [HttpPost("login/google/member")]
        public async Task<IActionResult> GoogleLoginAsMember([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            var response = await _authService.GoogleLoginAsMemberAsync(googleLoginDTO);

            if (!string.IsNullOrEmpty(response.TemporaryPassword))
            {
                return Ok(new ApiResponse<object>
                {
                    Message = "Account created successfully with Google login.",
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
                Message = "Google Member login successful.",
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Email and role cannot be empty."
                });

            var result = await _authService.AssignRoleToUserAsync(request.Email, request.Role);
            return Ok(new ApiResponse<object>
            {
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Role name cannot be empty."
                });

            var result = await _authService.AddRoleAsync(roleName);
            return Ok(new ApiResponse<object>
            {
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Invalid authorization header."
                });

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var response = await _authService.GoogleSetPasswordAsync(setPasswordDTO, token);

            return Ok(new ApiResponse<object>
            {
                Message = "Password set successfully.",
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
                return Unauthorized(new ApiResponse<object>
                {
                    Message = "Email claim not found in token."
                });

            var result = await _authService.LogoutAsync(email);
            return Ok(new ApiResponse<object>
            {
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "All fields are required."
                });
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<object>
                {
                    Message = "Email claim not found."
                });

            var result = await _authService.ChangePasswordAsync(email, request);
            return Ok(new ApiResponse<object>
            {
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
                return BadRequest(new ApiResponse<object>
                {
                    Message = "UserId and RefreshToken cannot be empty."
                });

            var result = await _authService.RefreshTokensAsync(request);
            if (result == null)
                return Unauthorized(new ApiResponse<object>
                {
                    Message = "Invalid refresh token."
                });

            return Ok(new ApiResponse<object>
            {
                Message = "Token refreshed successfully.",
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
            var result = await _authService.SoftDeleteAccountAsync(email);
            return Ok(new ApiResponse<object>
            {
                Message = result
            });
        }

        /// <summary>
        /// Restore a soft-deleted account
        /// </summary>
        [HttpPut("accounts/{email}/restore")]
        public async Task<IActionResult> RestoreAccount(string email)
        {
            var result = await _authService.RestoreAccountAsync(email);
            return Ok(new ApiResponse<object>
            {
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
            var result = await _authService.ForgotPassword(request);
            return Ok(new ApiResponse<object>
            {
                Message = result
            });
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _authService.ResetPassword(request);
            return Ok(new ApiResponse<object>
            {
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
            var users = await _authService.GetAllUsersAsync();
            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Message = "Users retrieved successfully.",
                Data = users
            });
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<object>
                {
                    Message = "User not found."
                });

            return Ok(new ApiResponse<object>
            {
                Message = "User retrieved successfully.",
                Data = user
            });
        }
    }
}
