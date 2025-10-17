using System;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace AIDefCom.Service.Services.RecordingService
{
    // Simple storage service for audio recordings
    public class RecordingStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly TimeSpan _defaultTtl;
        private readonly string _accountName;
        private readonly string _accountKey;

        public RecordingStorageService(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var connectionString = configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException("ConnectionStrings:AzureStorage is missing.");

            _containerName = configuration["Storage:Container"] ?? "recordings";

            if (!int.TryParse(configuration["Storage:SasExpiryMinutes"], out var minutes) || minutes <= 0)
            {
                minutes = 15;
            }
            _defaultTtl = TimeSpan.FromMinutes(minutes);

            _blobServiceClient = new BlobServiceClient(connectionString);

            // Try to parse account name and key for SAS signing fallback
            (_accountName, _accountKey) = ParseAccountFromConnectionString(connectionString);
        }

        // Create an upload SAS for a new blob path
        public async Task<(Uri uploadUri, string blobUrl, string blobPath)> CreateUploadSasAsync(string userId, string fileExt = "webm")
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required", nameof(userId));
            var ext = (fileExt ?? "webm").TrimStart('.');
            var blobPath = BuildBlobPath(userId, ext);

            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();

            var blobClient = container.GetBlobClient(blobPath);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobPath,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.Add(_defaultTtl)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

            var uploadUri = GenerateBlobSasUri(blobClient, sasBuilder);
            var blobUrl = blobClient.Uri.ToString();
            return (uploadUri, blobUrl, blobPath);
        }

        // Create a read-only SAS for an existing blob path
        public async Task<Uri> CreateReadSasAsync(string blobPath, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(blobPath)) throw new ArgumentException("blobPath is required", nameof(blobPath));

            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();

            var blobClient = container.GetBlobClient(blobPath);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobPath,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.Add(ttl <= TimeSpan.Zero ? _defaultTtl : ttl)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return GenerateBlobSasUri(blobClient, sasBuilder);
        }

        private static string BuildBlobPath(string userId, string ext)
        {
            return $"{userId}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}.{ext}";
        }

        private Uri GenerateBlobSasUri(BlobClient blobClient, BlobSasBuilder builder)
        {
            if (blobClient.CanGenerateSasUri)
            {
                return blobClient.GenerateSasUri(builder);
            }

            // Fallback: sign manually with shared key
            if (!string.IsNullOrEmpty(_accountName) && !string.IsNullOrEmpty(_accountKey))
            {
                var credential = new StorageSharedKeyCredential(_accountName, _accountKey);
                var sas = builder.ToSasQueryParameters(credential);
                var uriBuilder = new BlobUriBuilder(blobClient.Uri)
                {
                    Sas = sas
                };
                return uriBuilder.ToUri();
            }

            throw new InvalidOperationException("Cannot generate SAS URI for blob client.");
        }

        private static (string accountName, string accountKey) ParseAccountFromConnectionString(string conn)
        {
            string accountName = string.Empty;
            string accountKey = string.Empty;

            if (string.IsNullOrWhiteSpace(conn)) return (accountName, accountKey);

            var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                if (p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
                    accountName = p.Substring("AccountName=".Length);
                else if (p.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))
                    accountKey = p.Substring("AccountKey=".Length);
            }
            return (accountName, accountKey);
        }
    }
}
