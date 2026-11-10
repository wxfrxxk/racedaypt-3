using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    // Stores uploaded images in an Azure Blob Storage container.
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _container;

        public AzureBlobStorageService(string connectionString, string containerName)
        {
            _container = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            // Create the container on first use. Blobs are publicly readable so the browser can display them.
            await _container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string blobName = $"{folder}/{Guid.NewGuid():N}{extension}";

            BlobClient blob = _container.GetBlobClient(blobName);

            await using Stream stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });

            return blob.Uri.ToString();
        }
    }
}
