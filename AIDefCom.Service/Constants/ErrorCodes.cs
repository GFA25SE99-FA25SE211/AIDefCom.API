namespace AIDefCom.Service.Constants
{
    /// <summary>
    /// Centralized error codes for standardized error handling
    /// Format: DEF[XXX] where XXX is a 3-digit number
    /// </summary>
    public static class ErrorCodes
    {
        // ==================== VALIDATION ERRORS (DEF001-DEF099) ====================
        
        /// <summary>
        /// Validation failed for one or more fields
        /// </summary>
        public const string ValidationError = "DEF001";
        
        /// <summary>
        /// Model state validation failed
        /// </summary>
        public const string ModelValidationError = "DEF002";
        
        /// <summary>
        /// Required field is missing
        /// </summary>
        public const string RequiredFieldMissing = "DEF003";
        
        /// <summary>
        /// Invalid data format
        /// </summary>
        public const string InvalidFormat = "DEF004";
        
        /// <summary>
        /// Duplicate entry detected
        /// </summary>
        public const string DuplicateEntry = "DEF005";

        // ==================== AUTHORIZATION ERRORS (DEF100-DEF199) ====================
        
        /// <summary>
        /// User is not authorized to perform this action
        /// </summary>
        public const string Unauthorized = "DEF100";
        
        /// <summary>
        /// Authentication failed
        /// </summary>
        public const string AuthenticationFailed = "DEF101";
        
        /// <summary>
        /// Invalid or expired token
        /// </summary>
        public const string InvalidToken = "DEF102";
        
        /// <summary>
        /// Insufficient permissions
        /// </summary>
        public const string InsufficientPermissions = "DEF103";
        
        /// <summary>
        /// Account is locked or disabled
        /// </summary>
        public const string AccountLocked = "DEF104";

        // ==================== RESOURCE ERRORS (DEF200-DEF299) ====================
        
        /// <summary>
        /// The requested resource could not be found
        /// </summary>
        public const string NotFound = "DEF200";
        
        /// <summary>
        /// Resource already exists
        /// </summary>
        public const string ResourceExists = "DEF201";
        
        /// <summary>
        /// Resource is in use and cannot be deleted
        /// </summary>
        public const string ResourceInUse = "DEF202";
        
        /// <summary>
        /// Resource has been modified
        /// </summary>
        public const string ResourceModified = "DEF203";

        // ==================== DATABASE ERRORS (DEF300-DEF399) ====================
        
        /// <summary>
        /// A database error occurred
        /// </summary>
        public const string DatabaseError = "DEF300";
        
        /// <summary>
        /// Database connection failed
        /// </summary>
        public const string DatabaseConnectionFailed = "DEF301";
        
        /// <summary>
        /// Database transaction failed
        /// </summary>
        public const string DatabaseTransactionFailed = "DEF302";
        
        /// <summary>
        /// Database constraint violation
        /// </summary>
        public const string DatabaseConstraintViolation = "DEF303";
        
        /// <summary>
        /// Database timeout
        /// </summary>
        public const string DatabaseTimeout = "DEF304";

        // ==================== EXTERNAL SERVICE ERRORS (DEF400-DEF499) ====================
        
        /// <summary>
        /// Failed to communicate with an external service
        /// </summary>
        public const string ExternalServiceError = "DEF400";
        
        /// <summary>
        /// External API returned an error
        /// </summary>
        public const string ExternalApiError = "DEF401";
        
        /// <summary>
        /// External service timeout
        /// </summary>
        public const string ExternalServiceTimeout = "DEF402";
        
        /// <summary>
        /// External service unavailable
        /// </summary>
        public const string ExternalServiceUnavailable = "DEF403";

        // ==================== BUSINESS LOGIC ERRORS (DEF500-DEF599) ====================
        
        /// <summary>
        /// Business rule violation
        /// </summary>
        public const string BusinessRuleViolation = "DEF500";
        
        /// <summary>
        /// Invalid operation for current state
        /// </summary>
        public const string InvalidOperation = "DEF501";
        
        /// <summary>
        /// Operation conflict
        /// </summary>
        public const string OperationConflict = "DEF502";
        
        /// <summary>
        /// Precondition failed
        /// </summary>
        public const string PreconditionFailed = "DEF503";

        // ==================== FILE/STORAGE ERRORS (DEF600-DEF699) ====================
        
        /// <summary>
        /// File upload failed
        /// </summary>
        public const string FileUploadFailed = "DEF600";
        
        /// <summary>
        /// File not found
        /// </summary>
        public const string FileNotFound = "DEF601";
        
        /// <summary>
        /// Invalid file format
        /// </summary>
        public const string InvalidFileFormat = "DEF602";
        
        /// <summary>
        /// File size exceeds limit
        /// </summary>
        public const string FileSizeExceeded = "DEF603";
        
        /// <summary>
        /// Storage error
        /// </summary>
        public const string StorageError = "DEF604";

        // ==================== RATE LIMITING ERRORS (DEF700-DEF799) ====================
        
        /// <summary>
        /// Rate limit exceeded
        /// </summary>
        public const string RateLimitExceeded = "DEF700";
        
        /// <summary>
        /// Too many requests
        /// </summary>
        public const string TooManyRequests = "DEF701";

        // ==================== GENERAL/INTERNAL ERRORS (DEF900-DEF999) ====================
        
        /// <summary>
        /// An unexpected error occurred
        /// </summary>
        public const string InternalError = "DEF999";
        
        /// <summary>
        /// Service temporarily unavailable
        /// </summary>
        public const string ServiceUnavailable = "DEF998";
        
        /// <summary>
        /// Not implemented
        /// </summary>
        public const string NotImplemented = "DEF997";
        
        /// <summary>
        /// Configuration error
        /// </summary>
        public const string ConfigurationError = "DEF996";
    }
}
