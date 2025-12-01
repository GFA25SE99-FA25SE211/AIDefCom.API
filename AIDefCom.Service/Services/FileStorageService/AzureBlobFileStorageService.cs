using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.FileStorageService
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _documentFolderPrefix;

        public AzureBlobFileStorageService(IConfiguration configuration)
        {
            var conn = configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException("AzureStorage:ConnectionString is missing.");
            var containerName = configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException("AzureStorage:ContainerName is missing.");
            _documentFolderPrefix = configuration["AzureStorage:DocumentFolderPrefix"] ?? "documents/";

            var serviceClient = new BlobServiceClient(conn);
            _containerClient = serviceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<string> UploadPdfAsync(IFormFile file, string prefixFolder = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            // Validate file type
            if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
                throw new ArgumentException("Only PDF and Word documents (.pdf, .doc, .docx) are allowed.");

            // Generate unique name
            var safeName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "_")
                .Replace(":", "_")
                .Replace("/", "_");

            var blobName = $"{(string.IsNullOrWhiteSpace(prefixFolder) ? _documentFolderPrefix : prefixFolder)}{safeName}_{Guid.NewGuid():N}{extension}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Determine content type based on file extension
            var contentType = GetContentType(extension);

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "no-cache"
            };

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers });

            // Return the blob URI (for secured containers you may need SAS instead)
            return blobClient.Uri.ToString();
        }

        public async Task<string> GetDownloadUrlAsync(string blobUrl, int expiryMinutes = 60)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                throw new ArgumentException("Blob URL cannot be empty.");

            // Extract blob name from URL
            var uri = new Uri(blobUrl);
            var blobName = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1) // Skip container name
                .Aggregate((a, b) => $"{a}/{b}");

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException("File not found in storage.");

            // Generate SAS token
            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("BlobClient must be authorized with Shared Key credentials to generate SAS.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b", // b = blob
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Small buffer
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        private static string GetContentType(string extension)
        {
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }
    }
}
