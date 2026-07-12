using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.UploadMultiple;

public class UploadMultiplePhotosCommandHandler : IRequestHandler<UploadMultiplePhotosCommand, ApiResponse<List<PhotoMonitoringDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public UploadMultiplePhotosCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<PhotoMonitoringDto>>> Handle(UploadMultiplePhotosCommand command, CancellationToken cancellationToken)
    {
        var projectExists = await _unitOfWork.Projects.ExistsAsync(p => p.Id == command.Request.ProjectId);
        if (!projectExists)
            return ApiResponse<List<PhotoMonitoringDto>>.Fail("Project not found");

        var dtos = new List<PhotoMonitoringDto>();

        foreach (var file in command.Files)
        {
            var photoPath = await _fileStorageService.UploadFileAsync(file, "photos");

            var entity = new Domain.Entities.PhotoMonitoring
            {
                Id = Guid.NewGuid(),
                ProjectId = command.Request.ProjectId,
                Title = $"{command.Request.Title} - {file.FileName}",
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
            dtos.Add(_mapper.Map<PhotoMonitoringDto>(entity));
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<List<PhotoMonitoringDto>>.Ok(dtos, $"{command.Files.Count} photos uploaded successfully");
    }
}