using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.UserService.Validators;

public class UserRequestValidator : AbstractValidator<UserRequest>
{
    private readonly dbContext _context;

    public UserRequestValidator(dbContext context)
    {
        _context = context;
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MustAsync(BeValidEmailAsync).WithMessage("An account does not exist with this email address.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");

        RuleFor(x => x.Description)
            .NotNull().WithMessage("Description is required.")
            .MaximumLength(64).WithMessage("Description must not exceed 64 characters.");
        
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Nickname is required.")
            .NotNull().WithMessage("Nickname is required.")
            .MaximumLength(32).WithMessage("Nickname must not exceed 32 characters.")
            .MustAsync((request, nickname, cancellationToken) => BeValidNickAsync(request, nickname, cancellationToken))
            .WithMessage("This nickname is already in use.");
 
    }

    private async Task<bool> BeValidNickAsync(UserRequest request, string nickname, CancellationToken cancellationToken)
    {
        return !await _context.Users.AnyAsync(user => user.Nickname == nickname && user.Id != request.Id, cancellationToken);
    }
    
    private async Task<bool> BeValidEmailAsync(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email && u.Id != UserRequest.UserId, cancellationToken);
    }
}