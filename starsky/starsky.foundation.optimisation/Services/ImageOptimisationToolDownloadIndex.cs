using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IImageOptimisationToolDownloadIndex), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImageOptimisationToolDownloadIndex(IHttpClientHelper httpClientHelper, IWebLogger logger)
	: IImageOptimisationToolDownloadIndex
{
	public async Task<ImageOptimisationBinariesContainer> DownloadIndex(
		ImageOptimisationToolDownloadOptions options)
	{
		foreach ( var indexUrl in options.IndexUrls )
		{
			var apiResult = await httpClientHelper.ReadString(indexUrl);
			if ( !apiResult.Key )
			{
				continue;
			}

			return new ImageOptimisationBinariesContainer(apiResult.Value, indexUrl,
				options.BaseUrls, true);
		}

		logger.LogError($"[ImageOptimisationToolDownloadIndex] Index not found for {options.ToolName}");
		return new ImageOptimisationBinariesContainer(string.Empty, null, [], false);
	}
}
