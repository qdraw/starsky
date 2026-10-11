using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Services;

/// <summary>
///     SkiaSharp is the image library, switch library here
/// </summary>
[Service(typeof(IOverlayImageFactory), InjectionLifetime = InjectionLifetime.Scoped)]
public class OverlayImageFactory(ISelectorStorage selectorStorage, AppSettings appSettings)
	: IOverlayImageFactory
{
	public IOverlayImage Create()
	{
		return new OverlayImageSkia(selectorStorage, appSettings);
	}
}
