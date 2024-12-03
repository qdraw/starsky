using System.Text.Json;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.video.GetDependencies.Models;

public class BinaryIndex
{
	public string Architecture { get; set; }
	public string Url { get; set; }
	public string Sha256 { get; set; }
}

public class FfmpegBinariesIndex
{
	public List<BinaryIndex> Binaries { get; set; }
}

public class FfmpegBinariesContainer
{
	public FfmpegBinariesContainer(string apiResultValue, Uri? indexUrl, List<Uri> baseUrls,
		bool success)
	{
		Data = ParseIndex(apiResultValue);
		IndexUrl = indexUrl;
		BaseUrls = baseUrls;
		Success = success;
	}

	public List<Uri> BaseUrls { get; set; }

	public bool Success { get; set; }
	public Uri? IndexUrl { get; set; }
	public FfmpegBinariesIndex? Data { get; set; }

	private static FfmpegBinariesIndex? ParseIndex(string apiResultValue)
	{
		if ( string.IsNullOrEmpty(apiResultValue) )
		{
			return null;
		}

		return JsonSerializer.Deserialize<FfmpegBinariesIndex>(apiResultValue,
			DefaultJsonSerializer.CamelCase);
	}
}
