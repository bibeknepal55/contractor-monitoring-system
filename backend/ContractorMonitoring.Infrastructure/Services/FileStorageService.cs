using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8 } },
        { ".jpeg", new byte[] { 0xFF, 0xD8 } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46 } },
        { ".bmp", new byte[] { 0x42, 0x4D } },
        { ".webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } }
    };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"]
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        ValidateFile(file);

        var folderPath = Path.Combine(_basePath, SanitizeFolderName(folder));
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName).ToLower()}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine(SanitizeFolderName(folder), fileName).Replace("\\", "/");
    }

    public async Task<byte[]> DownloadFileAsync(string filePath)
    {
        // FIXED: Validate path stays within base directory
        var fullPath = GetSafePath(filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found", filePath);

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteFileAsync(string filePath)
    {
        // FIXED: Validate path stays within base directory
        var fullPath = GetSafePath(filePath);

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

    // FIXED: Validate file by extension AND magic bytes
    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds {MaxFileSize / 1024 / 1024}MB limit");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

        // FIXED: Validate magic bytes (file signature)
        if (MagicBytes.TryGetValue(extension, out var magic))
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[magic.Length];
            stream.Read(buffer, 0, magic.Length);

            if (!buffer.SequenceEqual(magic))
                throw new ArgumentException($"File content does not match its extension. File may be corrupted or disguised.");
        }
    }

    // FIXED: Prevent path traversal attacks
    private string GetSafePath(string relativePath)
    {
        // Sanitize the path
        var sanitized = relativePath.Replace('\\', '/').TrimStart('/');

        // Get full path
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, sanitized));

        // Verify it stays within base directory
        if (!fullPath.StartsWith(Path.GetFullPath(_basePath) + Path.DirectorySeparatorChar)
            && fullPath != Path.GetFullPath(_basePath))
        {
            throw new UnauthorizedAccessException("Access to the requested file path is denied.");
        }

        return fullPath;
    }

    // Sanitize folder name to prevent traversal
    private static string SanitizeFolderName(string folder)
    {
        // Remove any path traversal characters
        return folder
            .Replace("..", "")
            .Replace("/", "")
            .Replace("\\", "")
            .Trim();
    }
}