namespace ThesisBackend.Domain.Messages;

public class UserRequest
{
	public string Email { get; set; }
	public string Nickname { get; set; }
	public string Description { get; set; } = "No description provided";
	public string ImageLocation { get; set; } = "default.jpg";
}