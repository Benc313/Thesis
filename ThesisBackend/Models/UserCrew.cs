using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisBackend.Models;

public class UserCrew
{
	[Key]
	public int Id { get; set; }

	public int CrewId { get; set; }
	[ForeignKey("CrewId")]
	public Crew Crew { get; set; }

	public int UserId { get; set; }
	[ForeignKey("UserId")]
	public User User { get; set; }

	public Rank Rank { get; set; }
}