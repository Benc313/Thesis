using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThesisBackend.Messages;

namespace ThesisBackend.Models;

public class Meet
{
	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(64)]
	public string Name { get; set; }

	public string Description { get; set; }
	[Required]
	public int CreatorId { get; set; }
	public User Creator { get; set; }
	
	public int? CrewId { get; set; } = null;
	public Crew? Crew { get; set; } = null;
	
	[Required]
	[StringLength(128)]
	public string Location { get; set; }

	[Required]
	[StringLength(32)]
	public string Coordinates { get; set; }
	
	[Required]
	public DateTime Date { get; set; }
	
	[Required]
	public bool Private { get; set; }
	
	public List<MeetTags> Tags { get; set; } = new List<MeetTags>();

	public List<User> Users { get; set; } = new List<User>(); // Many-to-many with User
	
	[NotMapped]
	public double Latitude => double.Parse(Coordinates.Split(',')[0]);
	[NotMapped]
	public double Longitude => double.Parse(Coordinates.Split(',')[1]);
	
	public Meet() { }
	
	public Meet(MeetRequest meetRequest, User user, Crew crew)
	{
		Name = meetRequest.Name;
		Description = meetRequest.Description;
		CreatorId = user.Id;
		Creator = user;
		CrewId = meetRequest.CrewId;
		Crew = crew;
		Location = meetRequest.Location;
		Coordinates = meetRequest.Coordinates;
		Date = meetRequest.Date;
		Private = meetRequest.Private;
		Tags = meetRequest.Tags;
	}
	
	public void UpdateMeet(MeetRequest meetRequest, Crew crew)
	{
		Name = meetRequest.Name;
		Description = meetRequest.Description;
		CrewId = meetRequest.CrewId;
		Crew = crew;
		Location = meetRequest.Location;
		Coordinates = meetRequest.Coordinates;
		Date = meetRequest.Date;
		Private = meetRequest.Private;
		Tags = meetRequest.Tags;
	}
}