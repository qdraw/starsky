using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;

namespace starsky.feature.webhtmlpublish.Services;

public class OverlayImageSkia(ISelectorStorage selectorStorage, AppSettings appSettings)
	: OverlayImageBase(selectorStorage, appSettings)
{
	/// <summary>
	///     Matches the quality of the previous (ImageSharp) output, so published images stay similar
	/// </summary>
	internal const int JpegQuality = 75;

	protected override async Task<bool> ResizeOverlayImageShared(Stream sourceImageStream,
		Stream overlayImageStream, AppSettingsPublishProfiles profile, string outputSubPath)
	{
		using ( var oriented = SkiaImageHelper.DecodeAutoOrient(sourceImageStream) )
		using ( var sourceImage = SkiaImageHelper.Resize(oriented, profile.SourceMaxWidth, 0) )
		using ( var overlayOriginal = SkiaImageHelper.Decode(overlayImageStream) )
		using ( var overlayImage =
		       SkiaImageHelper.Resize(overlayOriginal, profile.OverlayMaxWidth, 0) )
		using ( var outputStream = new MemoryStream() )
		{
			var xPoint = sourceImage.Width - overlayImage.Width;
			var yPoint = sourceImage.Height - overlayImage.Height;

			SkiaImageHelper.DrawOverlay(sourceImage, overlayImage, xPoint, yPoint);

			SkiaImageHelper.Encode(sourceImage, SKEncodedImageFormat.Jpeg, JpegQuality,
				outputStream);
			outputStream.Seek(0, SeekOrigin.Begin);

			await HostFileSystem.WriteStreamAsync(outputStream, outputSubPath);
			return true;
		}
	}
}
