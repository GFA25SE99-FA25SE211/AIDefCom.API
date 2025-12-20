using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;
using System.Text.Json;

namespace AIDefCom.API.Middlewares
{
    /// <summary>
    /// Middleware to handle authentication and authorization errors
    /// Returns unified response format for 401 and 403 status codes
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Handle 401 Unauthorized responses
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized && !context.Response.HasStarted)
            {
                await HandleUnauthorizedAsync(context);
            }
            // Handle 403 Forbidden responses
            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden && !context.Response.HasStarted)
            {
                await HandleForbiddenAsync(context);
            }
        }

        private async Task HandleUnauthorizedAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            var hasToken = !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

            var message = hasToken 
                ? ResponseMessages.UnauthorizedInvalidToken 
                : ResponseMessages.UnauthorizedMissingToken;

            _logger.LogWarning(
                "Unauthorized access attempt to {Path}. HasToken: {HasToken}, IP: {IP}",
                context.Request.Path,
                hasToken,
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            var response = new ApiResponse<object>
            {
                Code = ResponseCodes.Unauthorized,
                Message = message,
                Data = null,
                Details = hasToken 
                    ? "Your session has expired or the token is invalid. Please login again to continue." 
                    : "This endpoint requires authentication. Please include a valid Bearer token in the Authorization header."
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private async Task HandleForbiddenAsync(HttpContext context)
        {
            var user = context.User;
            var userRoles = user.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var userName = user.Identity?.Name ?? "Anonymous";

            _logger.LogWarning(
                "Forbidden access attempt to {Path} by user {UserName} with roles: {Roles}",
                context.Request.Path,
                userName,
                string.Join(", ", userRoles)
            );

            var response = new ApiResponse<object>
            {
                Code = ResponseCodes.Forbidden,
                Message = ResponseMessages.Forbidden,
                Data = null,
                Details = userRoles.Any()
                    ? $"Your current role(s) ({string.Join(", ", userRoles)}) do not have permission to access this resource. Please contact your administrator if you believe this is an error."
                    : "You do not have the required role to access this resource. Please contact your administrator."
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
