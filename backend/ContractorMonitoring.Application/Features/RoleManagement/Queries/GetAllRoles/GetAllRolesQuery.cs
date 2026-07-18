using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RoleManagement;

namespace ContractorMonitoring.Application.Features.RoleManagement.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<ApiResponse<List<RoleDto>>>;