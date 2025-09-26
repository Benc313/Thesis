using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.CrewService.Interfaces;
using ThesisBackend.Services.CrewService.Models;

namespace ThesisBackend.Services.CrewService.Services;

public class CrewService : ICrewService
{
    private readonly dbContext _context;
    private readonly ILogger<CrewService> _logger;

    public CrewService(dbContext context, ILogger<CrewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CrewOperationResult> CreateCrewAsync(CrewRequest crewRequest, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return new CrewOperationResult { Success = false, ErrorMessage = "User not found." };
        }

        var newCrew = new Crew(crewRequest);
        var userCrew = new UserCrew { User = user, Crew = newCrew, Rank = Rank.Leader };

        _context.Crews.Add(newCrew);
        _context.UserCrews.Add(userCrew);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Crew created successfully: {CrewName}", newCrew.Name);
        return new CrewOperationResult { Success = true, Crew = new CrewResponse(newCrew) };
    }

    public async Task<CrewOperationResult> AddUserToCrewAsync(UserCrewRequest request, int crewId)
    {
        var crew = await _context.Crews.FindAsync(crewId);
        var user = await _context.Users.FindAsync(request.UserId);

        if (crew == null || user == null)
        {
            _logger.LogWarning("Crew or User not found. CrewId: {CrewId}, UserId: {UserId}", crewId, request.UserId);
            return new CrewOperationResult { Success = false, ErrorMessage = "Crew or User not found." };
        }

        var userCrew = new UserCrew { User = user, Crew = crew, Rank = request.Rank };
        _context.UserCrews.Add(userCrew);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} added to Crew {CrewId}", request.UserId, crewId);
        return new CrewOperationResult { Success = true };
    }
    
    
    public async Task<CrewOperationResult> RemoveUserFromCrewAsync(int crewId, int userId)
    {
        var userCrew = await _context.UserCrews
            .FirstOrDefaultAsync(uc => uc.CrewId == crewId && uc.UserId == userId);
        
        if (userCrew == null) return new CrewOperationResult { Success = false, ErrorMessage = "User is not in this crew." };

        _context.UserCrews.Remove(userCrew);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} removed from Crew {CrewId}", userId, crewId);
        return new CrewOperationResult { Success = true };
    }

    public async Task<CrewOperationResult> DeleteCrewAsync(int crewId)
    {
        var crew = await _context.Crews.FindAsync(crewId);
        if (crew == null) return new CrewOperationResult { Success = false, ErrorMessage = "Crew not found." };

        _context.Crews.Remove(crew);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Crew deleted: {CrewId}", crewId);
        return new CrewOperationResult { Success = true };
    }
    
    public async Task<CrewOperationResult> UpdateCrewAsync(CrewRequest crewRequest, int crewId)
    {
        var crew = await _context.Crews.FindAsync(crewId);
        if (crew == null)
        {
            _logger.LogWarning("Crew not found for update: {CrewId}", crewId);
            return new CrewOperationResult { Success = false, ErrorMessage = "Crew not found." };
        }

        crew.Name = crewRequest.Name;
        crew.Description = crewRequest.Description;
        crew.ImageLocation = crewRequest.ImageLocation;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Crew updated successfully: {CrewId}", crewId);
        return new CrewOperationResult { Success = true, Crew = new CrewResponse(crew) };
    }

    public async Task<CrewOperationResult> GetCrewAsync(int crewId)
    {
        var crew = await _context.Crews
            .Include(c => c.UserCrews)
            .ThenInclude(uc => uc.User)
            .FirstOrDefaultAsync(c => c.Id == crewId);

        if (crew == null)
        {
            _logger.LogWarning("Crew not found: {CrewId}", crewId);
            return new CrewOperationResult { Success = false, ErrorMessage = "Crew not found." };
        }

        return new CrewOperationResult { Success = true, Crew = new CrewResponse(crew) };
    }

    public async Task<AllCrewsOperationResult> GetAllCrewsAsync()
    {
        var crews = await _context.Crews
            .Include(c => c.UserCrews)
            .ThenInclude(uc => uc.User)
            .Select(c => new CrewResponse(c))
            .ToListAsync();

        return new AllCrewsOperationResult { Success = true, Crews = crews };
    }
}
