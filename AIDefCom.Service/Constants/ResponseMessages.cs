namespace AIDefCom.Service.Constants
{
    /// <summary>
    /// Unified response messages corresponding to ResponseCodes
    /// </summary>
    public static class ResponseMessages
    {
        // ==================== SUCCESS MESSAGES (2xx) ====================
        
        public const string Success = "Operation completed successfully.";
        public const string Created = "Resource created successfully.";
        public const string NoContent = "Operation completed successfully, no content returned.";
        public const string MultiStatus = "Operation completed with partial success. Some items succeeded, some failed.";
        
        // Resource-specific success messages (parameterized)
        public const string Retrieved = "{0} retrieved successfully.";
        public const string ListRetrieved = "{0} list retrieved successfully.";
        public const string Updated = "{0} updated successfully.";
        public const string Deleted = "{0} deleted successfully.";
        public const string Restored = "{0} restored successfully.";
        public const string Approved = "{0} approved successfully.";
        public const string Rejected = "{0} rejected successfully.";
        public const string SoftDeleted = "{0} soft deleted successfully.";
        public const string TotalScoreUpdated = "{0} total score updated successfully.";

        // ==================== CLIENT ERROR MESSAGES (4xx) ====================
        
        public const string BadRequest = "The request is invalid or cannot be processed.";
        public const string Unauthorized = "Authentication failed. Please provide a valid access token.";
        public const string UnauthorizedMissingToken = "Access token is required. Please login first.";
        public const string UnauthorizedInvalidToken = "Your access token is invalid or expired. Please login again.";
        public const string Forbidden = "Access denied. You do not have sufficient permissions to perform this action.";
        public const string ForbiddenRoleRequired = "Access denied. This action requires {0} role.";
        public const string NotFound = "The requested resource could not be found.";
        public const string Conflict = "The operation conflicts with the current state of the resource.";
        public const string RequestTimeout = "The request timed out.";
        public const string UnprocessableEntity = "The request contains semantic errors.";
        
        // Specific validation messages
        public const string ValidationFailed = "Validation failed for one or more fields.";
        public const string RequiredField = "Required field is missing: {0}";
        public const string InvalidFormat = "Invalid format for field: {0}";
        public const string DuplicateEntry = "A resource with this identifier already exists.";
        public const string ResourceInUse = "The resource is currently in use and cannot be deleted.";

        // ==================== SERVER ERROR MESSAGES (5xx) ====================
        
        public const string InternalError = "An unexpected error occurred while processing your request.";
        public const string NotImplemented = "This feature is not yet implemented.";
        public const string BadGateway = "Failed to communicate with an external service.";
        public const string ServiceUnavailable = "The service is temporarily unavailable. Please try again later.";
        public const string GatewayTimeout = "The request to an external service timed out.";
        
        // Specific error messages
        public const string DatabaseError = "A database error occurred while processing your request.";
        public const string DatabaseTimeout = "The database operation timed out.";
        public const string DatabaseConstraint = "The operation violates a database constraint.";
        public const string FileNotFound = "The requested file could not be found.";
        public const string StorageError = "A storage error occurred.";
        
        // New: host shutdown / crash
        public const string HostShutdown = "The server is shutting down or has encountered a fatal host error.";
        
        // ==================== IMPORT VALIDATION MESSAGES ====================
        
        public const string ImportValidationFailed = "Import validation failed. All rows must be valid before import can proceed.";
        public const string ImportSuccess = "Import completed successfully. {0} record(s) imported.";
        public const string ImportAllOrNothingFailed = "Import aborted. Found {0} validation error(s) in the file. Please fix all errors and try again.";
    }
}
