using System.Text.Json;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.optimisation.Models;

public class ImageOptimisationBinaryIndex
{
	public required string Architecture { get; set; }
	public required string FileName { get; set; }
	public required string Sha256 { get; set; }
}

public class ImageOptimisationBinariesIndex
{
	public required List<ImageOptimisationBinaryIndex> Binaries { get; set; }
}

public class ImageOptimisationBinariesContainer
{
	public ImageOptimisationBinariesContainer()
	{
		BaseUrls = [];
	}

	/// <summary>
	///     Makes a container for ImageOptimisation results
	/// </summary>
	/// <param name="apiResultValue">API results, must be valid JSON</param>
	/// <param name="indexUrl">where from</param>
	/// <param name="baseUrls">base domain</param>
	/// <param name="success">is success</param>
	/// <exception cref="JsonException">if apiResultValue is invalid</exception>
	public ImageOptimisationBinariesContainer(string apiResultValue, Uri? indexUrl,
		List<Uri> baseUrls, bool success)
	{
		Data = ParseIndex(apiResultValue);
		IndexUrl = indexUrl;
		BaseUrls = baseUrls;
		Success = success;
	}

	public List<Uri> BaseUrls { get; set; }
	public bool Success { get; set; }
	public Uri? IndexUrl { get; set; }
	public ImageOptimisationBinariesIndex? Data { get; set; }

	private static ImageOptimisationBinariesIndex? ParseIndex(string apiResultValue)
	{
		if ( string.IsNullOrEmpty(apiResultValue) )
		{
			return null;
		}

		// Can have a JsonException if input is invalid
		return JsonSerializer.Deserialize<ImageOptimisationBinariesIndex>(apiResultValue,
			DefaultJsonSerializer.CamelCase);
	}
}
