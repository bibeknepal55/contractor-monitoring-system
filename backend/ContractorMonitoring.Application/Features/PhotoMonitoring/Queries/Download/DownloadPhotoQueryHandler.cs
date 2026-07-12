using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.Download;

public class DownloadPhotoQueryHandler : IRequestHandler<DownloadPhotoQuery, ApiResponse<PhotoDownloadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DownloadPhotoQueryHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<PhotoDownloadDto>> Handle(DownloadPhotoQuery request, CancellationToken cancellationToken)
    {
        var photo = await _unitOfWork.PhotoMonitorings.GetByIdAsync(request.Id);
        if (photo == null)
            return ApiResponse<PhotoDownloadDto>.Fail("Photo not found");

        var fileBytes = await _fileStorageService.DownloadFileAsync(photo.PhotoPath);
        var extension = Path.GetExtension(photo.PhotoPath).ToLower();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var dto = new PhotoDownloadDto
        {
            FileBytes = fileBytes,
            FileName = Path.GetFileName(photo.PhotoPath),
            ContentType = contentType
        };

        return ApiResponse<PhotoDownloadDto>.Ok(dto, "Photo downloaded successfully");
    }
}