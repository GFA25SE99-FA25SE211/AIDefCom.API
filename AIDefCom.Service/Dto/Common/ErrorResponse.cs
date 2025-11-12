namespace AIDefCom.Service.Dto.Common
{
    /// <summary>
    /// Standardized error response structure for API errors
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Error code in DEF format (e.g., DEF001, DEF002)
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error information (only in Development environment)
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Additional context or metadata about the error
        /// </summary>
        public object? Metadata { get; set; }
    }
}
