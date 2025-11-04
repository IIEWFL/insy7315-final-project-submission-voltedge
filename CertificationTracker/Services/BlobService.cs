using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CertificationTracker.Services
{
    public class BlobService : IBlobService
    {
        private readonly string? _blobConnection;
        private readonly IConfiguration _config;

        public BlobService(IConfiguration config)
        {
            _config = config;
            _blobConnection = _config.GetValue<string>("Azure:BlobStorageConnectionString");
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName, string fileName)
        {
            var client = new BlobContainerClient(_blobConnection, containerName);
            await client.CreateIfNotExistsAsync(PublicAccessType.None);
            var blobClient = client.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(
                content: stream,
                httpHeaders: new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string blobUrl, string containerName)
        {
            var client = new BlobContainerClient(_blobConnection, containerName);
            var filename = Path.GetFileName(new Uri(blobUrl).LocalPath);
            var blobClient = client.GetBlobClient(filename);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
