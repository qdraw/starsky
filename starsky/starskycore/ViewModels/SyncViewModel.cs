using starskycore.Models;
#if NETSTANDARD2_1
using System.Text.Json;
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

#if NETSTANDARD2_1
		[JsonConverter(typeof(JsonStringEnumConverter))]
#else
		[JsonConverter(typeof(StringEnumConverter))]
#endif
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
