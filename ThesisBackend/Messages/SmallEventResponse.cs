using ThesisBackend.Models;

namespace ThesisBackend.Messages;

public class SmallEventResponse
{
	public int Id { get; set; }
	public string Name { get; set; }
	public DateTime Date { get; set; }
	public bool Private { get; set; }
	public bool IsMeet { get; set; }
	public SmallEventResponse(Meet meet)
	{
		Id = meet.Id;
		Name = meet.Name;
		Date = meet.Date;
		Private = meet.Private;
		IsMeet = true;
	}

	public SmallEventResponse(Race race)
	{
		Id = race.Id;
		Name = race.Name;
		Date = race.Date;
		Private = race.Private;
		IsMeet = false;
	}
}