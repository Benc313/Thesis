using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class CrewResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageLocation { get; set; }
    public List<UserResponse> Users { get; set; } = new();

    
    public CrewResponse() { }

    public CrewResponse(Crew crew)
    {
        Id = crew.Id;
        Name = crew.Name;
        Description = crew.Description;
        ImageLocation = crew.ImageLocation;
        if (crew.UserCrews != null)
        {
            Users = crew.UserCrews
                .Where(uc => uc.User != null)
                .Select(uc => new UserResponse(uc.User))
                .ToList();
        }
    }
}