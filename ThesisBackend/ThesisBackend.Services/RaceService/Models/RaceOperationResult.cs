using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.RaceService.Models;

public class RaceOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public RaceResponse? Race { get; set; }
}

public class AllRacesOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SmallEventResponse>? Races { get; set; }
}