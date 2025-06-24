using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models;

public class DiagnosticsItem
{
	[Key]
	[Column(TypeName = "varchar(150)")]
	[MaxLength(150)]
	[Required]
	public string Key { get; set; } = string.Empty;

	[MaxLength(4096)] [Required] public string Value { get; set; } = string.Empty;

	public DateTime LastEdited { get; set; }
}
