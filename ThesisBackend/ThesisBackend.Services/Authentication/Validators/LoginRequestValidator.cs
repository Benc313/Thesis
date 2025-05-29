using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.Authentication.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    private readonly dbContext _context;
    
    public LoginRequestValidator(dbContext context)
    {
        _context = context;
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required..")
            .MustAsync(BeValidEmailAsync).WithMessage("An account does not exist with this email address.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 8 characters long.");
    }
    
    private async Task<bool> BeValidEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}