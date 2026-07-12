using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Upload;

public class UploadPhotoCommandHandler : IRequestHandler<UploadPhotoCommand, ApiResponse<PhotoMonitoringDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public UploadPhotoCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PhotoMonitoringDto>> Handle(UploadPhotoCommand command, CancellationToken cancellationToken)
    {
        var projectExists = await _unitOfWork.Projects.ExistsAsync(p => p.Id == command.Request.ProjectId);
        if (!projectExists)
            return ApiResponse<PhotoMonitoringDto>.Fail("Project not found");

        // Upload file
        var photoPath = await _fileStorageService.UploadFileAsync(command.File, "photos");

        var entity = new Domain.Entities.PhotoMonitoring
        {
            Id = Guid.NewGuid(),
            ProjectId = command.Request.ProjectId,
            Title = command.Request.Title,
            Description = command.Request.Description,
            PhotoPath = photoPath,
            PhotoDate = command.Request.PhotoDate,
            Location = command.Request.Location,
            Direction = command.Request.Direction,
            PhotoType = command.Request.PhotoType ?? "Progress",
            Tags = command.Request.Tags,
            UploadedBy = command.UserName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.UserId.ToString(),
            TenantId = command.TenantId
        };

        await _unitOfWork.PhotoMonitorings.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PhotoMonitoringDto>(entity);
        return ApiResponse<PhotoMonitoringDto>.Ok(dto, "Photo uploaded successfully");
    }
}