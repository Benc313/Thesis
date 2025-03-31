using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThesisBackend.Messages;

namespace ThesisBackend.Models;

public class Car
{
	[Key]
	public int Id { get; set; }
	[Required]
	public int UserId { get; set; }
	public User User { get; set; }

	[Required]
	[StringLength(32)]
	public string Brand { get; set; }

	[Required]
	[StringLength(32)]
	public string Model { get; set; }

	public string Description { get; set; }
	[Required]
	[StringLength(32)]
	public string Engine { get; set; }
	[Required]
	public int HorsePower { get; set; }
	
	public Car()
	{
	}
	
	public Car(CarRequest carRequest, User user)
	{
		UserId = user.Id;
		User = user;
		Brand = carRequest.Brand;
		Model = carRequest.Model;
		Description = carRequest.Description;
		Engine = carRequest.Engine;
		HorsePower = carRequest.HorsePower;
	}
	
	public void UpdateCar(CarRequest carRequest)
    {
        Brand = carRequest.Brand;
        Model = carRequest.Model;
        Description = carRequest.Description;
        Engine = carRequest.Engine;
        HorsePower = carRequest.HorsePower;
    }
}
