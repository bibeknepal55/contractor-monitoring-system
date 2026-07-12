using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Commands.Update;

public class UpdatePerformanceBondCommandHandler : IRequestHandler<UpdatePerformanceBondCommand, ApiResponse<PerformanceBondDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePerformanceBondCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PerformanceBondDto>> Handle(UpdatePerformanceBondCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.PerformanceBonds.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<PerformanceBondDto>.Fail("PerformanceBond not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.PerformanceBonds.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<PerformanceBondDto>(entity);
        return ApiResponse<PerformanceBondDto>.Ok(dto, "PerformanceBond updated successfully");
    }
}
