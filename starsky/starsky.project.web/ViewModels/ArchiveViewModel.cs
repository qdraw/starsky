using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.project.web.ViewModels
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public sealed class ArchiveViewModel
	{
		public IEnumerable<FileIndexItem> FileIndexItems { get; set; } = [];

		public List<string> Breadcrumb { get; set; } = [];

		public RelativeObjects RelativeObjects { get; set; } = new();

		public string SearchQuery { get; set; } = string.Empty;

		/// <summary>
		/// Used PageType by react client
		/// </summary>
		public string PageType => PageViewType.PageType.Archive.ToString();

		public string SubPath { get; set; } = string.Empty;

		/// <summary>
		/// Count the number of files (collection setting is ignored for this value)
		/// </summary>
		public int CollectionsCount { get; set; }

		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Display only the current filter selection of ColorClasses
		/// </summary>
		public List<ColorClassParser.Color> ColorClassActiveList { get; set; } = [];

		/// <summary>
		/// Give back a list of all colorClasses that are used in this specific folder 
		/// </summary>
		public List<ColorClassParser.Color> ColorClassUsage { get; set; } = [];

		/// <summary>
		/// For display only
		/// </summary>
		public bool Collections { get; set; } = true;
	}
}
