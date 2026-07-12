using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Update;

public class UpdateResponsibleOfficialCommandHandler : IRequestHandler<UpdateResponsibleOfficialCommand, ApiResponse<ResponsibleOfficialDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateResponsibleOfficialCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ResponsibleOfficialDto>> Handle(UpdateResponsibleOfficialCommand command, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ResponsibleOfficials.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResponse<ResponsibleOfficialDto>.Fail("ResponsibleOfficial not found");

        _mapper.Map(command.Request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = command.UserId.ToString();

        await _unitOfWork.ResponsibleOfficials.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ResponsibleOfficialDto>(entity);
        return ApiResponse<ResponsibleOfficialDto>.Ok(dto, "ResponsibleOfficial updated successfully");
    }
}
