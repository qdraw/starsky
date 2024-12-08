using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starsky.foundation.video.GetDependencies;

public class FfMpegDownloadIndex(IHttpClientHelper httpClientHelper, IWebLogger logger)
{
	private const string QdrawMirrorDomain = "qdraw.nl/special/mirror/ffmpeg";
	private const string NetlifyMirrorDomain = "_____starsky-dependencies.netlify.app/ffmpeg";

	private static readonly Uri FfMpegApiBasePath = new($"https://{NetlifyMirrorDomain}/");
	private static readonly Uri FfMpegApiBasePathMirror = new($"https://{QdrawMirrorDomain}/");
	private static readonly List<Uri> BaseUris = [FfMpegApiBasePath, FfMpegApiBasePathMirror];

	private readonly Uri _ffMpegApiIndex = new($"https://{NetlifyMirrorDomain}/index.json");
	private readonly Uri _ffMpegApiIndexMirror = new($"https://{QdrawMirrorDomain}/index.json");

	public async Task<FfmpegBinariesContainer> DownloadIndex()
	{
		var apiResult = await httpClientHelper.ReadString(_ffMpegApiIndex);
		if ( apiResult.Key )
		{
			return new FfmpegBinariesContainer(apiResult.Value, _ffMpegApiIndex, BaseUris,
				true);
		}

		apiResult = await httpClientHelper.ReadString(_ffMpegApiIndexMirror);
		if ( apiResult.Key )
		{
			return new FfmpegBinariesContainer(apiResult.Value, _ffMpegApiIndexMirror,
				BaseUris, true);
		}

		logger.LogError("[FfMpegDownload] Index not found");
		return new FfmpegBinariesContainer(string.Empty, null, [], false);
	}
}
