using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisBackend.Models;

public class Meet
{
	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(64)]
	public string Name { get; set; }

	public string Description { get; set; }

	public int CreatorId { get; set; }
	[ForeignKey("CreatorId")]
	public User Creator { get; set; }

	public int? CrewId { get; set; }
	[ForeignKey("CrewId")]
	public Crew Crew { get; set; }

	[StringLength(128)]
	public string Location { get; set; }

	[StringLength(32)]
	public string Coordinates { get; set; }

	public List<User> Users { get; set; } = new List<User>(); // Many-to-many with User
}