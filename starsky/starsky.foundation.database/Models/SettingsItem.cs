using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace starsky.foundation.database.Models;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class SettingsItem
{
	[Key] 
	[Column(TypeName = "varchar(150)")]
	[MaxLength(150)]
	[Required]
	public string Key { get; set; } = null!;

	[MaxLength(4096)]
	[Required]
	public string Value { get; set; } = null!;

	public bool IsUserEditable { get; set; }

	public int? UserId { get; set; } = -1;
}
