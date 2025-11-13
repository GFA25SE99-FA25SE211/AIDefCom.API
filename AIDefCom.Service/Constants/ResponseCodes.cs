namespace AIDefCom.Service.Constants
{
    /// <summary>
    /// Unified response codes for all API operations (both success and errors)
    /// Format: DEF[XXX] where XXX follows HTTP status code convention
    /// 2xx = Success, 4xx = Client Error, 5xx = Server Error
    /// </summary>
    public static class ResponseCodes
    {
        // ==================== SUCCESS RESPONSES (DEF2xx) ====================
        
        /// <summary>
        /// Standard success response for GET operations
        /// </summary>
        public const string Success = "DEF200";
        
        /// <summary>
        /// Resource created successfully (POST)
        /// </summary>
        public const string Created = "DEF201";
        
        /// <summary>
        /// Operation completed successfully, no content to return (DELETE)
        /// </summary>
        public const string NoContent = "DEF204";

        // ==================== CLIENT ERROR RESPONSES (DEF4xx) ====================
        
        /// <summary>
        /// General validation error or bad request
        /// </summary>
        public const string BadRequest = "DEF400";
        
        /// <summary>
        /// Authentication required or failed
        /// </summary>
        public const string Unauthorized = "DEF401";
        
        /// <summary>
        /// Insufficient permissions
        /// </summary>
        public const string Forbidden = "DEF403";
        
        /// <summary>
        /// Resource not found
        /// </summary>
        public const string NotFound = "DEF404";
        
        /// <summary>
        /// Conflict - resource already exists or duplicate entry
        /// </summary>
        public const string Conflict = "DEF409";
        
        /// <summary>
        /// Request timeout
        /// </summary>
        public const string RequestTimeout = "DEF408";
        
        /// <summary>
        /// Unprocessable entity - semantic errors
        /// </summary>
        public const string UnprocessableEntity = "DEF422";

        // ==================== SERVER ERROR RESPONSES (DEF5xx) ====================
        
        /// <summary>
        /// General internal server error
        /// </summary>
        public const string InternalError = "DEF500";
        
        /// <summary>
        /// Feature not implemented
        /// </summary>
        public const string NotImplemented = "DEF501";
        
        /// <summary>
        /// External service error (bad gateway)
        /// </summary>
        public const string BadGateway = "DEF502";
        
        /// <summary>
        /// Service temporarily unavailable
        /// </summary>
        public const string ServiceUnavailable = "DEF503";
        
        /// <summary>
        /// Gateway timeout
        /// </summary>
        public const string GatewayTimeout = "DEF504";
    }
}
