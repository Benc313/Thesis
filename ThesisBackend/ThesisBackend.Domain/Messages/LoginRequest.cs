namespace ThesisBackend.Domain.Messages;

public class LoginRequest
{
	public string Email { get; set; }
	public string Password { get; set; }
}