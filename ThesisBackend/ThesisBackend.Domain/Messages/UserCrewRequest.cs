using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class UserCrewRequest
{
    public int UserId { get; set; }
    public Rank Rank { get; set; }

}