namespace ThesisBackend.Messages;

public class RegistrationRequest
{
	public string email { get; set; }
	public string nickname { get; set; }
	public string password { get; set; }
}