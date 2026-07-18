using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.CreateUser;

public record CreateUserCommand : IRequest<ApiResponse<UserManagementDto>>
{
    public CreateUserDto Request { get; init; } = null!;
    public Guid CreatedBy { get; init; }
    public Guid TenantId { get; init; }
}