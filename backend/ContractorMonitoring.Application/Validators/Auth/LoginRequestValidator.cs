using FluentValidation;
using ContractorMonitoring.Application.DTOs.Auth;

namespace ContractorMonitoring.Application.Validators.Auth;

// Validator for Login request
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}