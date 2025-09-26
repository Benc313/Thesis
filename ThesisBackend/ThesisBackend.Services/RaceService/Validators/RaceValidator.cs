using FluentValidation;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.RaceService.Validators;

public class RaceValidator : AbstractValidator<RaceRequest>
{
    public RaceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Coordinates).NotEmpty();
        RuleFor(x => x.Date).NotEmpty().GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.RaceType).IsInEnum();
    }
}