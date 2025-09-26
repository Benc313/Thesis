using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.MeetService.Models;

public class MeetOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public MeetResponse? MeetResponse { get; set; }
}

public class AllMeetsOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SmallEventResponse>? Meets { get; set; }
}
