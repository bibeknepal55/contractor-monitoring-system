using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.Download;

public record DownloadPhotoQuery : IRequest<ApiResponse<PhotoDownloadDto>>
{
    public Guid Id { get; init; }
}