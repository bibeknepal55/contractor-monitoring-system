using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Infrastructure.Services;

// Local file storage service implementation
public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string[] _allowedExtensions = {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
        ".jfif", ".svg", ".ico", ".tiff", ".tif", ".heic", ".heif"
    };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        ValidateFile(file);

        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine(folder, fileName).Replace("\\", "/");
    }

    public async Task<byte[]> DownloadFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found", filePath);

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string folder)
    {
        var filePaths = new List<string>();

        foreach (var file in files)
        {
            var path = await UploadFileAsync(file, folder);
            filePaths.Add(path);
        }

        return filePaths;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds {MaxFileSize / 1024 / 1024}MB limit");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", _allowedExtensions)}");
    }
}