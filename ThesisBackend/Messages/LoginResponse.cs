using ThesisBackend.Models;

namespace ThesisBackend.Messages;

public class LoginResponse
{
	public int Id { get; set; }
	public string Nickname { get; set; }
	
	public LoginResponse(User user)
	{
		Id = user.Id;
		Nickname = user.Nickname;
	}
}