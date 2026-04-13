using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class CrewMemberResponse : UserResponse
{
    public Rank? Rank { get; set; }

    public CrewMemberResponse() { }

    public CrewMemberResponse(User user, Rank rank) : base(user)
    {
        Rank = rank;
    }
}
