using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.Authentication.Validators;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    private readonly dbContext _context;
    
    public RegistrationRequestValidator(dbContext context)
    {
        _context = context;
        
        RuleFor(user => user.Nickname)
            .NotEmpty().WithMessage("Nickname is required.")
            .Length(3, 32).WithMessage("Nickname must be between 3 and 32 characters.")
            .MustAsync(BeUniqueNicknameAsync).WithMessage("Nickname already taken.");
        
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.")
            .MustAsync(BeUniqueEmailAsync).WithMessage("An account with this email already exists.");
        
        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
    
    private async Task<bool> BeUniqueNicknameAsync(string nickname, CancellationToken cancellationToken)
    {
        // Check if any user already has this nickname
        return !await _context.Users.AnyAsync(u => u.Nickname == nickname, cancellationToken);
    }

    private async Task<bool> BeUniqueEmailAsync(string email, CancellationToken cancellationToken)
    {
        // Check if any user already has this email
        return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}