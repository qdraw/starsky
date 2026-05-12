using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models.Account;

public class Role
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	public string? Code { get; set; }
	public string? Name { get; set; }
	public int? Position { get; set; }
}
