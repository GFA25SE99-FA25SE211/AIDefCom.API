using System.Text.Json.Serialization;

namespace AIDefCom.Service.Dto.DefenseReport
{
    /// <summary>
    /// Response for file upload operations
    /// </summary>
    public class FileUploadResponseDto
    {
        /// <summary>
        /// The permanent URL of the uploaded file in Azure Blob Storage
        /// </summary>
        [JsonPropertyName("fileUrl")]
        public string FileUrl { get; set; }

        /// <summary>
        /// Temporary download URL with SAS token (valid for specified duration)
        /// </summary>
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Number of minutes the download URL is valid for
        /// </summary>
        [JsonPropertyName("expiryMinutes")]
        public int ExpiryMinutes { get; set; }

        /// <summary>
        /// Original file name
        /// </summary>
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
    }
}
