using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ContractorOfficeDetail.Commands.Update;

public class UpdateContractorOfficeDetailCommandHandler : IRequestHandler<UpdateContractorOfficeDetailCommand, ApiResponse<ContractorOfficeDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateContractorOfficeDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ContractorOfficeDetailDto>> Handle(UpdateContractorOfficeDetailCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ContractorOfficeDetails.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<ContractorOfficeDetailDto>.Fail("Contractor not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.ContractorOfficeDetails.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ContractorOfficeDetailDto>(entity);
        return ApiResponse<ContractorOfficeDetailDto>.Ok(dto, "Contractor updated successfully");
    }
}