using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using starskycore.Models;

namespace starskycore.ViewModels
{
	public class SyncViewModel
	{
		public string FilePath { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
