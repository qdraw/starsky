using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IMozJpegDownload), InjectionLifetime = InjectionLifetime.Scoped)]
public class MozJpegDownload(IImageOptimisationToolDownload imageOptimisationToolDownload)
	: IMozJpegDownload
{
	private static readonly Uri NetlifyBaseUri = new("https://starsky-dependencies.netlify.app/mozjpeg/");
	private static readonly Uri QdrawMirrorBaseUri = new("https://qdraw.nl/special/mirror/mozjpeg/");

	private static readonly ImageOptimisationToolDownloadOptions Options = new()
	{
		ToolName = "mozjpeg",
		IndexUrls =
		[
			new Uri(NetlifyBaseUri, "index.json"),
			new Uri(QdrawMirrorBaseUri, "index.json")
		],
		BaseUrls = [NetlifyBaseUri, QdrawMirrorBaseUri],
		RunChmodOnUnix = true
	};

	public async Task<ImageOptimisationDownloadStatus> Download(string? architecture = null,
		int retryInSeconds = 15)
	{
		return await imageOptimisationToolDownload.Download(Options, architecture,
			retryInSeconds);
	}
}
