using ThesisBackend.Domain.Models;

namespace ThesisBackend.Domain.Messages;

public class CarResponse
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public string Brand { get; set; }
	public string Model { get; set; }
	public string Description { get; set; }
	public string Engine { get; set; }
	public int HorsePower { get; set; }
	
	public CarResponse(Car car)
	{
		Id = car.Id;
		UserId = car.UserId;
		Brand = car.Brand;
		Model = car.Model;
		Description = car.Description;
		Engine = car.Engine;
		HorsePower = car.HorsePower;
	}
}