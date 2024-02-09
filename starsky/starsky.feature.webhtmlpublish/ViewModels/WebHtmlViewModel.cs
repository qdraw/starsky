using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.ViewModels;

public class WebHtmlViewModel
{
	public string ItemName { get; set; } = string.Empty;

	/// <summary>
	/// Used with helpers
	/// </summary>
	public AppSettings AppSettings { get; set; } = new();

	/// <summary>
	/// Current profile
	/// </summary>
	public AppSettingsPublishProfiles CurrentProfile { get; set; } = new();

	/// <summary>
	/// Other profiles within the same group
	/// </summary>
	public List<AppSettingsPublishProfiles> Profiles { get; set; } = [];

	public string[] Base64ImageArray { get; set; } = [];

	public List<FileIndexItem> FileIndexItems { get; set; } = [];
}
