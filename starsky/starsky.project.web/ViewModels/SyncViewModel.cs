using System.Text.Json.Serialization;
using starsky.foundation.database.Models;

namespace starsky.project.web.ViewModels
{
	public sealed class SyncViewModel
	{
		public string FilePath { get; set; } = string.Empty;

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
