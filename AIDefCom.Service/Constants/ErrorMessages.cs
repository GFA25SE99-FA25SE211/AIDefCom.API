namespace AIDefCom.Service.Constants
{
    /// <summary>
    /// Centralized error messages corresponding to error codes
    /// </summary>
    public static class ErrorMessages
    {
        // ==================== VALIDATION ERRORS ====================
        
        public const string ValidationError = "Validation failed for one or more fields.";
        public const string ModelValidationError = "The submitted data is invalid.";
        public const string RequiredFieldMissing = "One or more required fields are missing.";
        public const string InvalidFormat = "The data format is invalid.";
        public const string DuplicateEntry = "A duplicate entry already exists.";

        // ==================== AUTHORIZATION ERRORS ====================
        
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string AuthenticationFailed = "Authentication failed. Please check your credentials.";
        public const string InvalidToken = "The provided token is invalid or has expired.";
        public const string InsufficientPermissions = "You do not have sufficient permissions to perform this action.";
        public const string AccountLocked = "Your account has been locked or disabled.";

        // ==================== RESOURCE ERRORS ====================
        
        public const string NotFound = "The requested resource could not be found.";
        public const string ResourceExists = "The resource already exists.";
        public const string ResourceInUse = "The resource is currently in use and cannot be deleted.";
        public const string ResourceModified = "The resource has been modified by another user.";

        // ==================== DATABASE ERRORS ====================
        
        public const string DatabaseError = "A database error occurred while processing your request.";
        public const string DatabaseConnectionFailed = "Failed to connect to the database.";
        public const string DatabaseTransactionFailed = "The database transaction failed.";
        public const string DatabaseConstraintViolation = "A database constraint was violated.";
        public const string DatabaseTimeout = "The database operation timed out.";

        // ==================== EXTERNAL SERVICE ERRORS ====================
        
        public const string ExternalServiceError = "Failed to communicate with an external service.";
        public const string ExternalApiError = "The external API returned an error.";
        public const string ExternalServiceTimeout = "The external service request timed out.";
        public const string ExternalServiceUnavailable = "The external service is currently unavailable.";

        // ==================== BUSINESS LOGIC ERRORS ====================
        
        public const string BusinessRuleViolation = "A business rule was violated.";
        public const string InvalidOperation = "This operation is not valid for the current state.";
        public const string OperationConflict = "A conflict occurred while processing the operation.";
        public const string PreconditionFailed = "The required precondition was not met.";

        // ==================== FILE/STORAGE ERRORS ====================
        
        public const string FileUploadFailed = "File upload failed.";
        public const string FileNotFound = "The requested file could not be found.";
        public const string InvalidFileFormat = "The file format is not supported.";
        public const string FileSizeExceeded = "The file size exceeds the maximum allowed limit.";
        public const string StorageError = "A storage error occurred.";

        // ==================== RATE LIMITING ERRORS ====================
        
        public const string RateLimitExceeded = "Rate limit exceeded. Please try again later.";
        public const string TooManyRequests = "Too many requests. Please slow down.";

        // ==================== GENERAL/INTERNAL ERRORS ====================
        
        public const string InternalError = "An unexpected error occurred while processing your request.";
        public const string ServiceUnavailable = "The service is temporarily unavailable. Please try again later.";
        public const string NotImplemented = "This feature is not yet implemented.";
        public const string ConfigurationError = "A configuration error occurred.";
    }
}
