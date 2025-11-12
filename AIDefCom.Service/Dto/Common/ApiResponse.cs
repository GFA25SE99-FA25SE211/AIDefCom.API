namespace AIDefCom.Service.Dto.Common
{
    /// <summary>
    /// Generic API response wrapper for consistent response structure
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Message code for categorization and i18n support
        /// Format: [Module].[Type][Code] (e.g., "Auth.Success0001", "Student.Fail0001")
        /// </summary>
        public string MessageCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable message describing the result of the operation
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The data payload
        /// </summary>
        public T? Data { get; set; }
    }
}
