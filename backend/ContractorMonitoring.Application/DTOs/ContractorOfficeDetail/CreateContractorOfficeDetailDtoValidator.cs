using FluentValidation;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

namespace ContractorMonitoring.Application.Validators.ContractorOfficeDetail;

public class CreateContractorOfficeDetailDtoValidator : AbstractValidator<CreateContractorOfficeDetailDto>
{
    public CreateContractorOfficeDetailDtoValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.ContactPerson).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactPersonPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ContactPersonEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactPersonEmail));
        RuleFor(x => x.Status).Must(x => new[] { "Active", "Inactive", "Blacklisted" }.Contains(x));
    }
}