using System.Text.Json.Serialization;
using starskycore.Models;

namespace starskycore.ViewModels
{
	public class SyncViewModel
	{
		public string FilePath { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
