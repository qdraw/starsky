using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.Models;

public class SettingsItem
{
	[Key] 
	public string? Key { get; set; }

	public string? Value { get; set; }

	public bool IsUserEditable { get; set; } = false;

	public int? UserId { get; set; } = -1;
}
