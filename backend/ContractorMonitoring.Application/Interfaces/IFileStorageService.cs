using Microsoft.AspNetCore.Http;

namespace ContractorMonitoring.Application.Interfaces;

// File storage service interface
public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task<byte[]> DownloadFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string folder);
}