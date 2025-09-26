using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Domain.Models;

public class Race
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
	[ForeignKey("CrewId")]
	
	public Crew? Crew { get; set; } = null;
	[Required]
	public RaceType RaceType { get; set; }
	
	[Required]
	[StringLength(128)]
	public string Location { get; set; }
	
	[Required]
	[StringLength(32)]
	public string Coordinates { get; set; }

	[Required]
	public bool Private { get; set; }
	
	[Required]
	public DateTime Date { get; set; }

	public List<User> Users { get; set; } = new List<User>(); // Many-to-many with User

	[NotMapped]
	public double Latitude => double.Parse(Coordinates.Split(',')[0]);
	[NotMapped]
	public double Longitude => double.Parse(Coordinates.Split(',')[1]);
	
	public Race()
	{
	}

	public Race(RaceRequest raceRequest, User creator, Crew? crew)
	{
		Name = raceRequest.Name;
		Description = raceRequest.Description;
		Creator = creator;
		CreatorId = creator.Id;
		if (crew != null)
		{
			Crew = crew;
			CrewId = crew.Id;
		}

		RaceType = raceRequest.RaceType;
		Location = raceRequest.Location;
		Coordinates = raceRequest.Coordinates;
		Private = raceRequest.Private;
		Date = raceRequest.Date;
	}

	public void UpdateRace(RaceRequest raceRequest, Crew? crew)
	{
		Name = raceRequest.Name;
		Description = raceRequest.Description;
		if (crew != null)
		{
			Crew = crew;
			CrewId = crew.Id;
		}
		RaceType = raceRequest.RaceType;
		Location = raceRequest.Location;
		Coordinates = raceRequest.Coordinates;
		Private = raceRequest.Private;
		Date = raceRequest.Date;
	}

}