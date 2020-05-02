using starsky.foundation.database.Models;
#if SYSTEM_TEXT_ENABLED
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif

namespace starskycore.ViewModels
{
	public class SyncViewModel
	{
		public string FilePath { get; set; }

#if SYSTEM_TEXT_ENABLED
		[JsonConverter(typeof(JsonStringEnumConverter))]
#else
		[JsonConverter(typeof(StringEnumConverter))]
#endif
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
