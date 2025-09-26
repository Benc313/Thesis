using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.MeetService.Models;

namespace ThesisBackend.Services.MeetService.Interfaces;

public interface IMeetService
{
    Task<MeetOperationResult> AddMeetAsync(MeetRequest meetRequest, int userId);
    Task<MeetOperationResult> UpdateMeetAsync(MeetRequest meetRequest, int meetId);
    Task<MeetOperationResult> GetMeetAsync(int meetId);
    Task<MeetOperationResult> DeleteMeetAsync(int meetId);
    Task<MeetOperationResult> JoinMeetAsync(int meetId, int userId);
    Task<AllMeetsOperationResult> GetAllMeetsAsync();
    Task<AllMeetsOperationResult> GetFilteredMeetsAsync(LocationQuery query);
}