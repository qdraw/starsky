namespace starsky.foundation.platform.Models;

public sealed class AppSettingsKeyValue
{
	public string Key { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;

	public void Deconstruct(out string key, out string value)
	{
		key = Key;
		value = Value;
	}
}
