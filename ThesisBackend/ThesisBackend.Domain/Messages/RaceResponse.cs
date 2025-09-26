using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class RaceResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CreatorId { get; set; }
    public int? CrewId { get; set; }
    public RaceType RaceType { get; set; }
    public string Location { get; set; }
    public string Coordinates { get; set; }
    public bool Private { get; set; }
    public DateTime Date { get; set; }
    public List<UserResponse> Users { get; set; } = new();
    
    public RaceResponse(){}
    
    public RaceResponse(Race race)
    {
        Id = race.Id;
        Name = race.Name;
        Description = race.Description;
        CreatorId = race.CreatorId;
        CrewId = race.CrewId;
        RaceType = race.RaceType;
        Location = race.Location;
        Coordinates = race.Coordinates;
        Private = race.Private;
        Date = race.Date;
        Users = race.Users.Select(u => new UserResponse(u)).ToList();
    }

}