namespace AIDefCom.Service.Dto.Common
{
    /// <summary>
    /// Unified API response structure for both success and error responses
    /// Uses standardized DEF codes for all operations
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Response code in DEF format
        /// Success: DEF2xx (200-299)
        /// Client Error: DEF4xx (400-499)
        /// Server Error: DEF5xx (500-599)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The data payload (null for errors)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Additional details (mainly for errors)
        /// </summary>
        public string? Details { get; set; }
    }
}
