using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.webhtmlpublish.Models
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public class PublishManifestModel
	{
		/// <summary>
		/// Display name
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Which files or folders need to be copied
		/// </summary>
		public Dictionary<string, bool> Copy { get; set; } = new();

		/// <summary>
		/// Slug, Name without spaces, but underscores are allowed
		/// </summary>
		public string Slug => GenerateSlugHelper.GenerateSlug(Name, true);

		/// <summary>
		/// When did the export happen
		/// </summary>
		public string Export =>
			DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

		/// <summary>
		/// Starsky Version
		/// </summary>
		public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();
	}
}
