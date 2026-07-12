using FluentValidation;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;

namespace ContractorMonitoring.Application.Validators.ContractFinancialDetail;

public class CreateContractFinancialDetailDtoValidator : AbstractValidator<CreateContractFinancialDetailDto>
{
    public CreateContractFinancialDetailDtoValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ContractAmount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.PaymentTerms).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BankName).MaximumLength(200);
    }
}