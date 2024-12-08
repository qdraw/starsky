using System.Runtime.CompilerServices;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfMpegDownloadIndex), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegDownloadIndex(IHttpClientHelper httpClientHelper, IWebLogger logger)
{
	private const string QdrawMirrorDomain = "qdraw.nl/special/mirror/ffmpeg";
	private const string NetlifyMirrorDomain = "_____starsky-dependencies.netlify.app/ffmpeg";

	private static readonly Uri FfMpegApiBasePath = new($"https://{NetlifyMirrorDomain}/");
	private static readonly Uri FfMpegApiBasePathMirror = new($"https://{QdrawMirrorDomain}/");
	private static readonly List<Uri> BaseUris = [FfMpegApiBasePath, FfMpegApiBasePathMirror];

	internal static readonly Uri FfMpegApiIndex = new($"https://{NetlifyMirrorDomain}/index.json");

	internal static readonly Uri FfMpegApiIndexMirror =
		new($"https://{QdrawMirrorDomain}/index.json");

	public async Task<FfmpegBinariesContainer> DownloadIndex()
	{
		var apiResult = await httpClientHelper.ReadString(FfMpegApiIndex);
		if ( apiResult.Key )
		{
			return new FfmpegBinariesContainer(apiResult.Value, FfMpegApiIndex, BaseUris,
				true);
		}

		apiResult = await httpClientHelper.ReadString(FfMpegApiIndexMirror);
		if ( apiResult.Key )
		{
			return new FfmpegBinariesContainer(apiResult.Value, FfMpegApiIndexMirror,
				BaseUris, true);
		}

		logger.LogError("[FfMpegDownloadIndex] Index not found");
		return new FfmpegBinariesContainer(string.Empty, null, [], false);
	}
}
