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
        public const string Unauthorized = "Authentication is required to access this resource.";
        public const string Forbidden = "You do not have permission to access this resource.";
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
    }
}
