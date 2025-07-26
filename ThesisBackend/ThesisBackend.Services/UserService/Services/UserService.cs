using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThesisBackend.Application.Authentication.Services;
using ThesisBackend.Application.UserService.Interfaces;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.UserService.Models;

namespace ThesisBackend.Services.UserService.Services;

public class UserService : IUserService
{
    private readonly dbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        dbContext context,
        ILogger<UserService> logger)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task<UserOperationResult> GetUser(int userId)
    {
        _logger.LogInformation("Attempting to get user with ID: {userId}", userId);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {userId} not found.", userId);
            return new UserOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        _logger.LogInformation("Successfully retrieved user with ID: {userId}", userId);
        var userForResponse = new UserResponse(user);
        return new UserOperationResult { Success = true, UserResponse = userForResponse};
    }
    
    public async Task<AllUserOperationResult> GetUsers()
    {
        _logger.LogInformation("Attempting to get all users...");
        var users = await _context.Users.ToListAsync();
        if (users == null || users.Count == 0)
        {
            _logger.LogWarning("No users found in the database.");
            return new AllUserOperationResult { Success = false, ErrorMessage = "No users found" };
        }
        _logger.LogInformation("Successfully retrieved all users.");
        return new AllUserOperationResult{ UserResponse = users.Select(user => new UserResponse(user)).ToList(), Success = true};
    }
    
    public async Task<UserOperationResult> UpdateUser(UserRequest userRequest, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {userId} not found for update.", userId);
            return new UserOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        _logger.LogInformation("Attempting to update user {userId}.", userId);
        user.UpdateUser(userRequest);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} updated successfully.", userId);
        var userForResponse = new UserResponse(user);
        return new UserOperationResult { Success = true, UserResponse = userForResponse};
    }
    
    public async Task<UserEventOperationResult> GetUserMeets(int userId)
    {
        _logger.LogInformation("Attempting to get meets for user with ID: {userId}", userId);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {userId} not found.", userId);
            return new UserEventOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        _logger.LogInformation("Successfully retrieved user with ID: {userId}", userId);
        var events = await _context.Meets
            .Include(m => m.Users)
            .Where(m => m.Users.Any(u => u.Id == userId) || m.CreatorId == userId)
            .ToListAsync();
        if (events.Count == 0)
        {
            _logger.LogWarning("No meets found for user with ID {userId} found.", userId);
            return new UserEventOperationResult { Success = false, ErrorMessage = "Events not found for this user" };
        }
        _logger.LogInformation("Successfully retrieved meets for user with ID: {userId}", userId);
        return new UserEventOperationResult
            { SmallEventResponse = events.Select(e => new SmallEventResponse(e)).ToList(), Success = true};
    }
    
    public async Task<UserEventOperationResult> GetUserRaces(int userId)
    {
        _logger.LogInformation("Attempting to get user races with ID: {userId}", userId);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {userId} not found.", userId);
            return new UserEventOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        _logger.LogInformation("Successfully retrieved user races with ID: {userId}", userId);
        var events = await _context.Races
            .Include(r => r.Users)
            .Where(m => m.Users.Any(u => u.Id == userId))
            .ToListAsync();
        if (events.Count == 0)
        {
            _logger.LogWarning("No races for user with ID {userId} found.", userId);
            return new UserEventOperationResult { Success = false, ErrorMessage = "Events not found for this user" };
        }
        _logger.LogInformation("Successfully retrieved races for user with ID: {userId}", userId);
        return new UserEventOperationResult
            { SmallEventResponse = events.Select(e => new SmallEventResponse(e)).ToList(), Success = true};
    }
}