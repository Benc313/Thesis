using FluentValidation;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.MeetService.Validators;

public class MeetRequestValidator : AbstractValidator<MeetRequest>
{
    public MeetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64).WithMessage("Name must not exceed 64 characters.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(128).WithMessage("Location must not exceed 128 characters.");

        RuleFor(x => x.Coordinates)
            .NotEmpty().WithMessage("Coordinates are required.")
            .Matches(@"^-?\d+(\.\d+)?,-?\d+(\.\d+)?$")
            .WithMessage("Invalid coordinate format. Expected format: 'latitude,longitude'.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .GreaterThan(DateTime.Now).WithMessage("Date must be in the future.");

        RuleFor(x => x.Tags)
            .NotEmpty().WithMessage("At least one tag is required.");
    }
}