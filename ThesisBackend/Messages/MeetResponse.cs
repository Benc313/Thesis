using ThesisBackend.Models;

namespace ThesisBackend.Messages;

public class MeetResponse
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int CreatorId { get; set; }
	public int? CrewId { get; set; } = null;
	public string Location { get; set; }
	public string Coordinates { get; set; }
	public DateTime Date { get; set; }
	public bool Private { get; set; }
	public List<MeetTags> Tags { get; set; } = new List<MeetTags>();
	
	public MeetResponse(Meet meet)
	{
		Id = meet.Id;
		Name = meet.Name;
		Description = meet.Description;
		CreatorId = meet.CreatorId;
		CrewId = meet.CrewId;
		Location = meet.Location;
		Coordinates = meet.Coordinates;
		Date = meet.Date;
		Private = meet.Private;
		Tags = meet.Tags;
	}
}