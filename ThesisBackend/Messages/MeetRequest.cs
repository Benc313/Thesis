using ThesisBackend.Models;

namespace ThesisBackend.Messages;

public class MeetRequest
{
	public string Name { get; set; }
	public string Description { get; set; }
	public int? CrewId { get; set; } = null;
	public string Location { get; set; }
	public string Coordinates { get; set; }
	public DateTime Date { get; set; }
	public bool Private { get; set; }
	public List<MeetTags> Tags { get; set; } = new List<MeetTags>();
}