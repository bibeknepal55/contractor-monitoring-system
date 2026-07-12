using FluentValidation;
using ContractorMonitoring.Application.DTOs.Project;

namespace ContractorMonitoring.Application.Validators.Project;

// Validator for creating a project
public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.ProjectCode)
            .NotEmpty().WithMessage("Project code is required")
            .MaximumLength(50).WithMessage("Project code must not exceed 50 characters")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Project code must contain only uppercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(x => new[] { "Planned", "InProgress", "Completed", "OnHold", "Cancelled" }.Contains(x))
            .WithMessage("Invalid status. Allowed: Planned, InProgress, Completed, OnHold, Cancelled");

        RuleFor(x => x.Budget)
            .GreaterThan(0).WithMessage("Budget must be greater than 0");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.ProjectManager)
            .MaximumLength(150).WithMessage("Project manager name must not exceed 150 characters");

        RuleFor(x => x.ContactNumber)
            .MaximumLength(20).WithMessage("Contact number must not exceed 20 characters");

        RuleFor(x => x.ContractNumber)
            .MaximumLength(100).WithMessage("Contract number must not exceed 100 characters");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .Must(x => new[] { "Low", "Medium", "High", "Critical" }.Contains(x))
            .WithMessage("Invalid priority. Allowed: Low, Medium, High, Critical");

        RuleFor(x => x.ContractorId)
            .NotEmpty().WithMessage("Contractor is required");
    }
}
