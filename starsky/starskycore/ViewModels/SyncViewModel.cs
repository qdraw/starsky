using starsky.foundation.database.Models;
using System.Text.Json.Serialization;

namespace starskycore.ViewModels
{
	public sealed class SyncViewModel
	{
		public string FilePath { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
