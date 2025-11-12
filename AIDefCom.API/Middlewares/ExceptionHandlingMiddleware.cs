using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;
using AIDefCom.Service.Constants;
using AIDefCom.Service.Dto.Common;

namespace AIDefCom.API.Middlewares
{
    /// <summary>
    /// Global exception handling middleware with standardized DEF error codes
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
            var (errorCode, message, statusCode, logLevel) = MapExceptionToError(exception);

            // Log the exception with appropriate level
            LogException(exception, logLevel, errorCode);

            // Build error response
            var errorResponse = new ErrorResponse
            {
                ErrorCode = errorCode,
                Message = message
            };

            // Add details only in Development environment
            var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (environment.IsDevelopment())
            {
                errorResponse.Details = exception.Message;
                errorResponse.Metadata = new
                {
                    ExceptionType = exception.GetType().Name,
                    StackTrace = exception.StackTrace?.Split('\n').Take(5).ToArray(), // First 5 lines
                    InnerException = exception.InnerException?.Message
                };
            }

            // Set response
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = environment.IsDevelopment()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }

        private (string errorCode, string message, HttpStatusCode statusCode, LogLevel logLevel) MapExceptionToError(Exception exception)
        {
            return exception switch
            {
                // Validation Exceptions
                ArgumentNullException _ => (
                    ErrorCodes.RequiredFieldMissing,
                    ErrorMessages.RequiredFieldMissing,
                    HttpStatusCode.BadRequest,
                    LogLevel.Warning
                ),
                ArgumentException _ => (
                    ErrorCodes.ValidationError,
                    ErrorMessages.ValidationError,
                    HttpStatusCode.BadRequest,
                    LogLevel.Warning
                ),

                // Authorization Exceptions
                UnauthorizedAccessException _ => (
                    ErrorCodes.Unauthorized,
                    ErrorMessages.Unauthorized,
                    HttpStatusCode.Unauthorized,
                    LogLevel.Warning
                ),

                // Resource Exceptions
                KeyNotFoundException _ => (
                    ErrorCodes.NotFound,
                    ErrorMessages.NotFound,
                    HttpStatusCode.NotFound,
                    LogLevel.Information
                ),

                // Database Exceptions - SQL Server specific
                SqlException ex when ex.Number == -2 => ( // Timeout
                    ErrorCodes.DatabaseTimeout,
                    ErrorMessages.DatabaseTimeout,
                    HttpStatusCode.RequestTimeout,
                    LogLevel.Error
                ),
                SqlException ex when ex.Number == 2627 || ex.Number == 2601 => ( // Unique constraint
                    ErrorCodes.DatabaseConstraintViolation,
                    ErrorMessages.DatabaseConstraintViolation,
                    HttpStatusCode.Conflict,
                    LogLevel.Warning
                ),
                SqlException _ => (
                    ErrorCodes.DatabaseError,
                    ErrorMessages.DatabaseError,
                    HttpStatusCode.InternalServerError,
                    LogLevel.Error
                ),

                // Business Logic Exceptions
                InvalidOperationException ex when ex.Message.Contains("already exists") || ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) => (
                    ErrorCodes.DuplicateEntry,
                    ErrorMessages.DuplicateEntry,
                    HttpStatusCode.Conflict,
                    LogLevel.Warning
                ),
                InvalidOperationException _ => (
                    ErrorCodes.InvalidOperation,
                    ErrorMessages.InvalidOperation,
                    HttpStatusCode.Conflict,
                    LogLevel.Warning
                ),

                // External Service Exceptions
                HttpRequestException ex when ex.StatusCode == HttpStatusCode.RequestTimeout => (
                    ErrorCodes.ExternalServiceTimeout,
                    ErrorMessages.ExternalServiceTimeout,
                    HttpStatusCode.GatewayTimeout,
                    LogLevel.Error
                ),
                HttpRequestException ex when ex.StatusCode == HttpStatusCode.ServiceUnavailable => (
                    ErrorCodes.ExternalServiceUnavailable,
                    ErrorMessages.ExternalServiceUnavailable,
                    HttpStatusCode.BadGateway,
                    LogLevel.Error
                ),
                HttpRequestException _ => (
                    ErrorCodes.ExternalServiceError,
                    ErrorMessages.ExternalServiceError,
                    HttpStatusCode.BadGateway,
                    LogLevel.Error
                ),

                // File/IO Exceptions
                FileNotFoundException _ => (
                    ErrorCodes.FileNotFound,
                    ErrorMessages.FileNotFound,
                    HttpStatusCode.NotFound,
                    LogLevel.Warning
                ),
                IOException _ => (
                    ErrorCodes.StorageError,
                    ErrorMessages.StorageError,
                    HttpStatusCode.InternalServerError,
                    LogLevel.Error
                ),

                // Not Implemented
                NotImplementedException _ => (
                    ErrorCodes.NotImplemented,
                    ErrorMessages.NotImplemented,
                    HttpStatusCode.NotImplemented,
                    LogLevel.Warning
                ),

                // Default - Internal Server Error
                _ => (
                    ErrorCodes.InternalError,
                    ErrorMessages.InternalError,
                    HttpStatusCode.InternalServerError,
                    LogLevel.Error
                )
            };
        }

        private void LogException(Exception exception, LogLevel logLevel, string errorCode)
        {
            var logMessage = "Error {ErrorCode}: {ExceptionType} - {Message}";

            switch (logLevel)
            {
                case LogLevel.Warning:
                    _logger.LogWarning(exception, logMessage, errorCode, exception.GetType().Name, exception.Message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, logMessage, errorCode, exception.GetType().Name, exception.Message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(exception, logMessage, errorCode, exception.GetType().Name, exception.Message);
                    break;
                default:
                    _logger.LogError(exception, logMessage, errorCode, exception.GetType().Name, exception.Message);
                    break;
            }
        }
    }
}
