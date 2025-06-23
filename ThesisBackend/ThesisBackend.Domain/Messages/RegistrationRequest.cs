namespace ThesisBackend.Domain.Messages;

public class RegistrationRequest
{
	public string Email { get; set; }
	public string Nickname { get; set; }
	public string Password { get; set; }
	
}