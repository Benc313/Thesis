using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}