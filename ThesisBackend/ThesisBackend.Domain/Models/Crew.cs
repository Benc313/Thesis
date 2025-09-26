using System.ComponentModel.DataAnnotations;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Domain.Models;

public class Crew
{
	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(32)]
	public string Name { get; set; }

	public string Description { get; set; }

	[StringLength(64)]
	public string ImageLocation { get; set; }

	public List<UserCrew> UserCrews { get; set; } = new List<UserCrew>();
	public List<Meet> Meets { get; set; } = new List<Meet>();
	
	public Crew(){}

	public Crew(CrewRequest crewRequest)
	{
		Name = crewRequest.Name;
		Description = crewRequest.Description;
		ImageLocation = crewRequest.ImageLocation;
	}
}