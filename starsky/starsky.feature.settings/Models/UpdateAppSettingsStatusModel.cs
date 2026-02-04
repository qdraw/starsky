namespace starsky.feature.settings.Models;

public class UpdateAppSettingsStatusModel
{
	public int StatusCode { get; set; } = 200;

	public bool IsError => StatusCode >= 400;

	public string Message { get; set; } = string.Empty;
}
