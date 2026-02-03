using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using starsky.foundation.platform.Helpers.Slug;

namespace starsky.feature.webhtmlpublish.Models;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("Usage", "S2325:Mark members as static")]
public class PublishManifestModel
{
	/// <summary>
	///     Display name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///     Which files or folders need to be copied
	/// </summary>
	public Dictionary<string, bool> Copy { get; set; } = new();

	/// <summary>
	///     Slug, Name without spaces, but underscores are allowed
	/// </summary>
	public string Slug => GenerateSlugHelper.GenerateSlug(Name, true);

	/// <summary>
	///     When did the export happen
	/// </summary>
	public string Export =>
		DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

	/// <summary>
	///     Starsky Version
	/// </summary>
	public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

	/// <summary>
	///     The publishing profile name used for this manifest
	///     v0.7.11 or newer
	/// </summary>
	public string? PublishProfileName { get; set; } = string.Empty;
}
