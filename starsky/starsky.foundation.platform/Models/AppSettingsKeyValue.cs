namespace starsky.foundation.platform.Models;

public sealed class AppSettingsKeyValue
{
	public string Key { get; set; }
	public string Value { get; set; }
	
	public void Deconstruct(out string key, out string value)
	{
		key = Key;
		value = Value;
	}
}
