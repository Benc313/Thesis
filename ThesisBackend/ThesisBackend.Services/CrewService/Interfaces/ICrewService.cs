using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.CrewService.Models;

namespace ThesisBackend.Services.CrewService.Interfaces;

public interface ICrewService
{
    Task<CrewOperationResult> CreateCrewAsync(CrewRequest crewRequest, int userId);
    Task<CrewOperationResult> UpdateCrewAsync(CrewRequest crewRequest, int crewId);
    Task<CrewOperationResult> GetCrewAsync(int crewId);
    Task<AllCrewsOperationResult> GetAllCrewsAsync();
    Task<CrewOperationResult> AddUserToCrewAsync(UserCrewRequest request, int crewId);
}