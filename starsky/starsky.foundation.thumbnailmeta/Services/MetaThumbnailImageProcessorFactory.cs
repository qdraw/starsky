using starsky.foundation.injection;
using starsky.foundation.thumbnailmeta.Interfaces;

namespace starsky.foundation.thumbnailmeta.Services;

/// <summary>
///     SkiaSharp is the image library, switch library here
/// </summary>
[Service(typeof(IMetaThumbnailImageProcessorFactory),
	InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaThumbnailImageProcessorFactory : IMetaThumbnailImageProcessorFactory
{
	public IMetaThumbnailImageProcessor Create()
	{
		return new SkiaMetaThumbnailImageProcessor();
	}
}
