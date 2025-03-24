using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisBackend.Models;

public class Car
{
	[Key]
	public int Id { get; set; }

	public int UserId { get; set; }
	[ForeignKey("UserId")]
	public User User { get; set; }

	[Required]
	[StringLength(32)]
	public string Brand { get; set; }

	[Required]
	[StringLength(32)]
	public string Model { get; set; }

	[StringLength(256)]
	public string Description { get; set; }

	[StringLength(32)]
	public string Engine { get; set; }

	public int HorsePower { get; set; }
}
