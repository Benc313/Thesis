using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class UserResponse
{
	public string Id { get; set; }
	public string Email { get; set; }
	public string Nickname { get; set; }
	public string Description { get; set; } = "No description provided";
	public string ImageLocation { get; set; } = "default.jpg";
	
	public UserResponse() { }
	
	public UserResponse(User user)
	{
		Id = user.Id.ToString();
		Email = user.Email;
		Nickname = user.Nickname;
		Description = user.Description  ?? "No description provided";
		ImageLocation = user.ImageLocation ?? "default.jpg";
	}
}