using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.RaceService.Interfaces;
using ThesisBackend.Services.RaceService.Models;

namespace ThesisBackend.Services.RaceService.Services;

public class RaceService : IRaceService
{
    private readonly dbContext _context;
    private readonly ILogger<RaceService> _logger;

    public RaceService(dbContext context, ILogger<RaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RaceOperationResult> GetRaceAsync(int raceId)
    {
        var race = await _context.Races.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == raceId);
        if (race == null)
        {
            return new RaceOperationResult { Success = false, ErrorMessage = "Race not found" };
        }
        return new RaceOperationResult { Success = true, Race = new RaceResponse(race) };
    }

    public async Task<RaceOperationResult> AddRaceAsync(RaceRequest raceRequest, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new RaceOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        
        var race = new Race
        {
            Name = raceRequest.Name,
            Description = raceRequest.Description,
            Creator = user,
            CreatorId = user.Id,
            CrewId = raceRequest.CrewId,
            RaceType = raceRequest.RaceType,
            Location = raceRequest.Location,
            Coordinates = raceRequest.Coordinates,
            Private = raceRequest.Private,
            Date = raceRequest.Date
        };

        _context.Races.Add(race);
        await _context.SaveChangesAsync();
        return new RaceOperationResult { Success = true, Race = new RaceResponse(race) };
    }

    public async Task<RaceOperationResult> UpdateRaceAsync(RaceRequest raceRequest, int raceId)
    {
        var race = await _context.Races.FindAsync(raceId);
        if (race == null)
        {
            return new RaceOperationResult { Success = false, ErrorMessage = "Race not found" };
        }
        Crew crew = null;
        if (raceRequest.CrewId != null)
        {
            crew = _context.Crews.FirstOrDefault(c => c.Id == raceRequest.CrewId);
        }

        race.UpdateRace(raceRequest, crew);
        
        await _context.SaveChangesAsync();
        return new RaceOperationResult { Success = true, Race = new RaceResponse(race) };
    }

    public async Task<RaceOperationResult> DeleteRaceAsync(int raceId)
    {
        var race = await _context.Races.FindAsync(raceId);
        if (race == null)
        {
            return new RaceOperationResult { Success = false, ErrorMessage = "Race not found" };
        }

        _context.Races.Remove(race);
        await _context.SaveChangesAsync();
        return new RaceOperationResult { Success = true };
    }

    public async Task<RaceOperationResult> JoinRaceAsync(int raceId, int userId)
    {
        var race = await _context.Races.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == raceId);
        var user = await _context.Users.FindAsync(userId);

        if (race == null) return new RaceOperationResult { Success = false, ErrorMessage = "Race not found" };
        if (user == null) return new RaceOperationResult { Success = false, ErrorMessage = "User not found" };

        race.Users.Add(user);
        await _context.SaveChangesAsync();
        return new RaceOperationResult { Success = true };
    }

    public async Task<AllRacesOperationResult> GetAllRacesAsync()
    {
        var races = await _context.Races
            .Select(race => new SmallEventResponse(race))
            .ToListAsync();
        return new AllRacesOperationResult { Success = true, Races = races };
    }
    
    public async Task<AllRacesOperationResult> GetFilteredRacesAsync(LocationQuery query)
    {
        _logger.LogInformation("Retrieving filtered races with query: {@Query}", query);

        var queryable = _context.Races.AsQueryable();

        queryable = queryable.Where(m => m.Date >= DateTime.UtcNow.Date);

        var potentialRaces = await queryable.Select(m => new { m.Id, m.Name, m.Date, m.Private, m.Latitude, m.Longitude }).ToListAsync();

        var filteredRaces = potentialRaces
            .Where(m =>
                CalculateDistance(m.Latitude, m.Longitude, query.Latitude, query.Longitude) <= query.DistanceInKm
            )
            .Select(m => new SmallEventResponse {Id = m.Id, Name = m.Name, Date = m.Date, Private = m.Private, IsMeet = false })
            .ToList();

        return new AllRacesOperationResult { Success = true, Races = filteredRaces };
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