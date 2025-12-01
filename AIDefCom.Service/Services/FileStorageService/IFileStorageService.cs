using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.FileStorageService
{
    public interface IFileStorageService
    {
        Task<string> UploadPdfAsync(IFormFile file, string prefixFolder = null);
        
        /// <summary>
        /// Get a download URL with SAS token for a blob
        /// </summary>
        /// <param name="blobUrl">The full URL of the blob</param>
        /// <param name="expiryMinutes">How long the SAS token should be valid (default: 60 minutes)</param>
        /// <returns>URL with SAS token for downloading</returns>
        Task<string> GetDownloadUrlAsync(string blobUrl, int expiryMinutes = 60);
    }
}
