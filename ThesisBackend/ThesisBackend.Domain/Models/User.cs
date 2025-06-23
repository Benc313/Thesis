using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Domain.Models;

public class User
{
	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(320)]
	public string Email { get; set; }

	[Required]
	[StringLength(32)]
	public string Nickname { get; set; }

	[Required]
	[StringLength(320)]
	public string PasswordHash { get; set; }

	public string Description { get; set; } = "No description provided";

	[StringLength(64)]
	public string ImageLocation { get; set; } = "default.jpg";

	public List<Car> Cars { get; set; } = new List<Car>();
	public List<UserCrew> UserCrews { get; set; } = new List<UserCrew>();
	public List<Race> Races { get; set; } = new List<Race>(); // Many-to-many with Race
	public List<Meet> Meets { get; set; } = new List<Meet>(); // Many-to-many with Meet
	public List<Race> CreatedRaces { get; set; } = new List<Race>(); // Races created by this user
	public List<Meet> CreatedMeets { get; set; } = new List<Meet>(); // Meets created by this user
	
	public User() { }

	public User(RegistrationRequest registrationRequest, string hashedPassword)
	{
		Nickname = registrationRequest.Nickname;
		Email = registrationRequest.Email;
		PasswordHash = hashedPassword;
	}
	
	public void UpdateUser(UserRequest userRequest)
	{
		Email = userRequest.Email;
		Nickname = userRequest.Nickname;
		Description = userRequest.Description;
		ImageLocation = userRequest.ImageLocation;
	}
}