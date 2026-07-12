using FluentValidation;
using ContractorMonitoring.Application.DTOs.Project;

namespace ContractorMonitoring.Application.Validators.Project;

// Validator for updating a project
public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.EndDate)
            .GreaterThan(DateTime.Now).When(x => x.EndDate.HasValue)
            .WithMessage("End date must be in the future");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(x => new[] { "Planned", "InProgress", "Completed", "OnHold", "Cancelled" }.Contains(x))
            .WithMessage("Invalid status. Allowed: Planned, InProgress, Completed, OnHold, Cancelled");

        RuleFor(x => x.Budget)
            .GreaterThan(0).WithMessage("Budget must be greater than 0");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .Must(x => new[] { "Low", "Medium", "High", "Critical" }.Contains(x))
            .WithMessage("Invalid priority. Allowed: Low, Medium, High, Critical");

        RuleFor(x => x.ContractorId)
            .NotEmpty().WithMessage("Contractor is required");
    }
}