using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.MeetService.Interfaces;
using ThesisBackend.Services.MeetService.Models;

namespace ThesisBackend.Services.MeetService.Services;

public class MeetService : IMeetService
{
    private readonly dbContext _context;
    private readonly ILogger<MeetService> _logger;

    public MeetService(dbContext context, ILogger<MeetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MeetOperationResult> AddMeetAsync(MeetRequest meetRequest, int userId)
    {
        _logger.LogInformation("Attempting to add a new meet for user {UserId}", userId);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", userId);
            return new MeetOperationResult { Success = false, ErrorMessage = "User not found" };
        }

        var crew = meetRequest.CrewId.HasValue ? await _context.Crews.FindAsync(meetRequest.CrewId.Value) : null;
        if (meetRequest.CrewId.HasValue && crew == null)
        {
            _logger.LogWarning("Crew not found with ID: {CrewId}", meetRequest.CrewId);
            return new MeetOperationResult { Success = false, ErrorMessage = "Crew not found" };
        }

        var newMeet = new Meet(meetRequest, user, crew);
        _context.Meets.Add(newMeet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Meet {MeetName} created successfully by user {UserId}", newMeet.Name, userId);
        return new MeetOperationResult { Success = true, MeetResponse = new MeetResponse(newMeet) };
    }

    public async Task<MeetOperationResult> UpdateMeetAsync(MeetRequest meetRequest, int meetId)
    {
        _logger.LogInformation("Attempting to update meet with ID: {MeetId}", meetId);
        var meet = await _context.Meets.FindAsync(meetId);
        if (meet == null)
        {
            _logger.LogWarning("Meet not found with ID: {MeetId}", meetId);
            return new MeetOperationResult { Success = false, ErrorMessage = "Meet not found" };
        }

        var crew = meetRequest.CrewId.HasValue ? await _context.Crews.FindAsync(meetRequest.CrewId.Value) : null;
        if (meetRequest.CrewId.HasValue && crew == null)
        {
             _logger.LogWarning("Crew not found with ID: {CrewId}", meetRequest.CrewId);
            return new MeetOperationResult { Success = false, ErrorMessage = "Crew not found" };
        }

        meet.UpdateMeet(meetRequest, crew);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Meet with ID {MeetId} updated successfully", meetId);

        return new MeetOperationResult { Success = true, MeetResponse = new MeetResponse(meet) };
    }

    public async Task<MeetOperationResult> GetMeetAsync(int meetId)
    {
        _logger.LogInformation("Attempting to retrieve meet with ID: {MeetId}", meetId);
        var meet = await _context.Meets.Include(m => m.Users).FirstOrDefaultAsync(m => m.Id == meetId);
        if (meet == null)
        {
            _logger.LogWarning("Meet not found with ID: {MeetId}", meetId);
            return new MeetOperationResult { Success = false, ErrorMessage = "Meet not found" };
        }

        return new MeetOperationResult { Success = true, MeetResponse = new MeetResponse(meet, meet.Users.ToList()) };
    }

    public async Task<MeetOperationResult> DeleteMeetAsync(int meetId)
    {
        _logger.LogInformation("Attempting to delete meet with ID: {MeetId}", meetId);
        var meet = await _context.Meets.FindAsync(meetId);
        if (meet == null)
        {
            _logger.LogWarning("Meet not found with ID: {MeetId}", meetId);
            return new MeetOperationResult { Success = false, ErrorMessage = "Meet not found" };
        }

        _context.Meets.Remove(meet);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Meet with ID {MeetId} deleted successfully", meetId);

        return new MeetOperationResult { Success = true };
    }

    public async Task<MeetOperationResult> JoinMeetAsync(int meetId, int userId)
    {
        _logger.LogInformation("User {UserId} attempting to join meet {MeetId}", userId, meetId);
        var meet = await _context.Meets.Include(m => m.Users).FirstOrDefaultAsync(m => m.Id == meetId);
        if (meet == null)
        {
            _logger.LogWarning("Meet not found with ID: {MeetId}", meetId);
            return new MeetOperationResult { Success = false, ErrorMessage = "Meet not found" };
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", userId);
            return new MeetOperationResult { Success = false, ErrorMessage = "User not found" };
        }

        if (meet.Users.Any(u => u.Id == userId))
        {
            _logger.LogWarning("User {UserId} already in meet {MeetId}", userId, meetId);
            return new MeetOperationResult { Success = false, ErrorMessage = "User already joined the meet" };
        }

        meet.Users.Add(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} successfully joined meet {MeetId}", userId, meetId);

        return new MeetOperationResult { Success = true };
    }

    public async Task<AllMeetsOperationResult> GetAllMeetsAsync()
    {
        _logger.LogInformation("Retrieving all meets");
        var meets = await _context.Meets.Select(meet => new SmallEventResponse(meet)).ToListAsync();
        return new AllMeetsOperationResult { Success = true, Meets = meets };
    }

    public async Task<AllMeetsOperationResult> GetFilteredMeetsAsync(LocationQuery query)
    {
        _logger.LogInformation("Retrieving filtered meets with query: {@Query}", query);
        var meets = await _context.Meets.ToListAsync();

        var filteredMeets = meets
            .Where(m =>
                CalculateDistance(m.Latitude, m.Longitude, query.Latitude, query.Longitude) <= query.DistanceInKm &&
                (query.Tags.Count == 0 || m.Tags.Any(t => query.Tags.Contains(t.ToString()))) &&
                m.Date >= DateTime.Today
            )
            .Select(meet => new SmallEventResponse(meet))
            .ToList();

        return new AllMeetsOperationResult { Success = true, Meets = filteredMeets };
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

}