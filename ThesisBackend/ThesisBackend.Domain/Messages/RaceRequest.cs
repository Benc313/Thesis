using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class RaceRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int? CrewId { get; set; } = null;
    public RaceType RaceType { get; set; }
    public string Location { get; set; }
    public string Coordinates { get; set; }
    public bool Private { get; set; }
    public DateTime Date { get; set; }
}