using FluentValidation;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.CrewService.Validators;

public class CrewRequestValidator : AbstractValidator<CrewRequest>
{
    public CrewRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Crew name is required.")
            .Length(3, 32).WithMessage("Crew name must be between 3 and 32 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}