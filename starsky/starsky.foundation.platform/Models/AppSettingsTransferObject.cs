using System.Collections.Generic;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.platform.Models
{
	/// <summary>
	/// Used to update AppSettings from the UI 'preferences'
	/// </summary>
	public sealed class AppSettingsTransferObject
	{
		public bool? Verbose { get; set; }

		public string? StorageFolder { get; set; }
		public bool? UseSystemTrash { get; set; }

		public bool? UseLocalDesktop { get; set; }

		public List<AppSettingsDefaultEditorApplication> DefaultDesktopEditor { get; set; } = [];

		public CollectionsOpenType.RawJpegMode DesktopCollectionsOpen { get; set; } =
			CollectionsOpenType.RawJpegMode.Default;

	}
}
