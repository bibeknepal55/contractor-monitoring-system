using MediatR;
using Microsoft.AspNetCore.Http;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Upload;

public record UploadPhotoCommand : IRequest<ApiResponse<PhotoMonitoringDto>>
{
    public IFormFile File { get; init; } = null!;
    public UploadPhotoDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string UserName { get; init; } = string.Empty;
}