using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CertificationTracker.Services
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName, string fileName);
        Task DeleteFileAsync(string blobUrl, string containerName);
    }
}