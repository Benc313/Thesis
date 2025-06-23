using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisBackend.Domain.Models;

public class UserCrew
{
	[Key]
	public int Id { get; set; }
	
	[Required]
	public int CrewId { get; set; }
	
	public Crew Crew { get; set; }
	
	[Required]
	public int UserId { get; set; }
	
	[ForeignKey("UserId")]
	public User User { get; set; }
	
	[Required]

	public Rank Rank { get; set; }
}