using FluentValidation;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.CarService.Validators;

public class CarRequestValidator : AbstractValidator<CarRequest>
{
    private readonly dbContext _context;
    
    public CarRequestValidator(dbContext context)
    {
        _context = context;
        
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .NotNull().WithMessage("Brand is required.")
            .MaximumLength(32).WithMessage("Brand must not exceed 32 characters.");
        
        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .NotNull().WithMessage("Model is required.")
            .MaximumLength(32).WithMessage("Model must not exceed 32 characters.");
        
        RuleFor(x => x.Description)
            .MaximumLength(320).WithMessage("Description must not exceed 320 characters.");

        RuleFor(x => x.Engine)
            .NotEmpty().WithMessage("Engine is required.")
            .NotNull().WithMessage("Engine is required.")
            .MaximumLength(32).WithMessage("Engine must not exceed 32 characters.");
        
        RuleFor(x => x.HorsePower)
            .NotEmpty().WithMessage("HorsePower is required.")
            .NotNull().WithMessage("HorsePower is required.")
            .GreaterThan(0).WithMessage("HorsePower must be greater than 0.");
        
    }
}