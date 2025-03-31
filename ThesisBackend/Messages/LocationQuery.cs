namespace ThesisBackend.Messages;

public class LocationQuery
{
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public double DistanceInKm { get; set; } = 10; // Default to 10km
	public List<string> Tags { get; set; } = new List<string>();
}