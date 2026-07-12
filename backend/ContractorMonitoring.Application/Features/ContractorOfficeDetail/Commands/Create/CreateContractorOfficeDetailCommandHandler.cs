using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Create;

public class CreateContractorOfficeDetailCommandHandler : IRequestHandler<CreateContractorOfficeDetailCommand, ApiResponse<ContractorOfficeDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateContractorOfficeDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ContractorOfficeDetailDto>> Handle(CreateContractorOfficeDetailCommand command, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.ContractorOfficeDetails
            .ExistsAsync(c => c.RegistrationNumber == command.Request.RegistrationNumber || c.TaxId == command.Request.TaxId);

        if (exists)
            return ApiResponse<ContractorOfficeDetailDto>.Fail("Contractor with this registration number or tax ID already exists");

        var entity = _mapper.Map<Domain.Entities.ContractorOfficeDetail>(command.Request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = command.UserId.ToString();
        entity.TenantId = command.TenantId;
        entity.IsDeleted = false;

        await _unitOfWork.ContractorOfficeDetails.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ContractorOfficeDetailDto>(entity);
        return ApiResponse<ContractorOfficeDetailDto>.Ok(dto, "Contractor created successfully");
    }
}