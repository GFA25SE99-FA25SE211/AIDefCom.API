using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;
using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;

namespace AIDefCom.API.Middlewares
{
    /// <summary>
    /// Global exception handling middleware with unified DEF response codes
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Determine error details based on exception type
            var (code, message, statusCode, logLevel) = MapExceptionToResponse(exception);

            // Log the exception with appropriate level
            LogException(exception, logLevel, code);

            // Build unified response for errors
            var response = new ApiResponse<object>
            {
                Code = code,
                Message = message,
                Data = null,
                Details = exception.Message
            };

            // Set HTTP response
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private (string code, string message, HttpStatusCode statusCode, LogLevel logLevel) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                // Validation Exceptions ? DEF400
                ArgumentNullException ex => (
                    ResponseCodes.BadRequest,
                    string.Format(ResponseMessages.RequiredField, ex.ParamName ?? "field"),
                    HttpStatusCode.BadRequest,
                    LogLevel.Warning
                ),
                ArgumentException _ => (
                    ResponseCodes.BadRequest,
                    ResponseMessages.ValidationFailed,
                    HttpStatusCode.BadRequest,
                    LogLevel.Warning
                ),

                // Authorization Exceptions ? DEF401
                UnauthorizedAccessException _ => (
                    ResponseCodes.Unauthorized,
                    ResponseMessages.Unauthorized,
                    HttpStatusCode.Unauthorized,
                    LogLevel.Warning
                ),

                // Resource Not Found ? DEF404
                KeyNotFoundException _ => (
                    ResponseCodes.NotFound,
                    ResponseMessages.NotFound,
                    HttpStatusCode.NotFound,
                    LogLevel.Information
                ),

                // Database Timeout ? DEF408
                SqlException ex when ex.Number == -2 => (
                    ResponseCodes.RequestTimeout,
                    ResponseMessages.DatabaseTimeout,
                    HttpStatusCode.RequestTimeout,
                    LogLevel.Error
                ),

                // Database Constraint ? DEF409
                SqlException ex when ex.Number == 2627 || ex.Number == 2601 => (
                    ResponseCodes.Conflict,
                    ResponseMessages.DatabaseConstraint,
                    HttpStatusCode.Conflict,
                    LogLevel.Warning
                ),

                // General Database Error ? DEF500
                SqlException _ => (
                    ResponseCodes.InternalError,
                    ResponseMessages.DatabaseError,
                    HttpStatusCode.InternalServerError,
                    LogLevel.Error
                ),

                // Business Logic - Duplicate ? DEF409
                InvalidOperationException ex when ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                                                   ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) => (
                    ResponseCodes.Conflict,
                    ResponseMessages.DuplicateEntry,
                    HttpStatusCode.Conflict,
                    LogLevel.Warning
                ),

                // Business Logic - General ? DEF409
                InvalidOperationException _ => (
                    ResponseCodes.Conflict,
                    ResponseMessages.Conflict,
                    HttpStatusCode.Conflict,
                    LogLevel.Warning
                ),

                // External Service Timeout ? DEF504
                HttpRequestException ex when ex.StatusCode == HttpStatusCode.RequestTimeout => (
                    ResponseCodes.GatewayTimeout,
                    ResponseMessages.GatewayTimeout,
                    HttpStatusCode.GatewayTimeout,
                    LogLevel.Error
                ),

                // External Service Unavailable ? DEF503
                HttpRequestException ex when ex.StatusCode == HttpStatusCode.ServiceUnavailable => (
                    ResponseCodes.ServiceUnavailable,
                    ResponseMessages.ServiceUnavailable,
                    HttpStatusCode.ServiceUnavailable,
                    LogLevel.Error
                ),

                // External Service Error ? DEF502
                HttpRequestException _ => (
                    ResponseCodes.BadGateway,
                    ResponseMessages.BadGateway,
                    HttpStatusCode.BadGateway,
                    LogLevel.Error
                ),

                // File Not Found ? DEF404
                FileNotFoundException _ => (
                    ResponseCodes.NotFound,
                    ResponseMessages.FileNotFound,
                    HttpStatusCode.NotFound,
                    LogLevel.Warning
                ),

                // Storage Error ? DEF500
                IOException _ => (
                    ResponseCodes.InternalError,
                    ResponseMessages.StorageError,
                    HttpStatusCode.InternalServerError,
                    LogLevel.Error
                ),

                // Not Implemented ? DEF501
                NotImplementedException _ => (
                    ResponseCodes.NotImplemented,
                    ResponseMessages.NotImplemented,
                    HttpStatusCode.NotImplemented,
                    LogLevel.Warning
                ),

                // Default - Internal Server Error ? DEF500
                _ => (
                    ResponseCodes.InternalError,
                    ResponseMessages.InternalError,
                    HttpStatusCode.InternalServerError,
                    LogLevel.Error
                )
            };
        }

        private void LogException(Exception exception, LogLevel logLevel, string code)
        {
            var logMessage = "Response {Code}: {ExceptionType} - {Message}";

            switch (logLevel)
            {
                case LogLevel.Warning:
                    _logger.LogWarning(exception, logMessage, code, exception.GetType().Name, exception.Message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, logMessage, code, exception.GetType().Name, exception.Message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(exception, logMessage, code, exception.GetType().Name, exception.Message);
                    break;
                default:
                    _logger.LogError(exception, logMessage, code, exception.GetType().Name, exception.Message);
                    break;
            }
        }
    }
}
