using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.CrewService.Models;

namespace ThesisBackend.Services.CrewService.Interfaces;

public interface ICrewService
{
    Task<CrewOperationResult> CreateCrewAsync(CrewRequest crewRequest, int userId);
    Task<CrewOperationResult> UpdateCrewAsync(CrewRequest crewRequest, int crewId);
    Task<CrewOperationResult> GetCrewAsync(int crewId);
    Task<AllCrewsOperationResult> GetAllCrewsAsync();
    Task<CrewOperationResult> AddUserToCrewAsync(UserCrewRequest request, int crewId);
    Task<CrewOperationResult> RemoveUserFromCrewAsync(int crewId, int userId);
    Task<CrewOperationResult> DeleteCrewAsync(int crewId);
    Task<EventsForCrewOperationResult> GetEventsForCrewAsync(int crewId);
    Task<CrewOperationResult> UpdateUserRankAsync(int crewId, int userId, Rank rank);
}