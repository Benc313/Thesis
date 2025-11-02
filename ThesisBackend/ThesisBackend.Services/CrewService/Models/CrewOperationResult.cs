using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.CrewService.Models;

public class CrewOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public CrewResponse? Crew { get; set; }
}

public class AllCrewsOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<CrewResponse>? Crews { get; set; }
}

public class EventsForCrewOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SmallEventResponse>? Events { get; set; }
}
