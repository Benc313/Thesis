using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.RaceService.Models;

namespace ThesisBackend.Services.RaceService.Interfaces;

public interface IRaceService
{
    Task<RaceOperationResult> GetRaceAsync(int raceId);
    Task<RaceOperationResult> AddRaceAsync(RaceRequest raceRequest, int userId);
    Task<RaceOperationResult> UpdateRaceAsync(RaceRequest raceRequest, int raceId);
    Task<RaceOperationResult> DeleteRaceAsync(int raceId);
    Task<RaceOperationResult> JoinRaceAsync(int raceId, int userId);
    Task<AllRacesOperationResult> GetAllRacesAsync();
    Task<AllRacesOperationResult> GetFilteredRacesAsync(LocationQuery query);

}