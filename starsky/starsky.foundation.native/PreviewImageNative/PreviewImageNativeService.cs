using System.Runtime.InteropServices;
using starsky.foundation.injection;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.native.PreviewImageNative;

[Service(typeof(IPreviewImageNativeService), InjectionLifetime = InjectionLifetime.Scoped)]
public class PreviewImageNativeService(IWebLogger logger) : IPreviewImageNativeService
{
	public bool GeneratePreviewImage(string filePath, string outputPath, int width, int height)
	{
		return GeneratePreviewImage(RuntimeInformation.IsOSPlatform,
			filePath,
			outputPath,
			width,
			height);
	}

	internal bool GeneratePreviewImage(
		IsOsPlatformDelegate runtimeInformationIsOsPlatform,
		string filePath, string outputPath, int width, int height)
	{
		// Linux or Windows is not supported yet
		if ( !runtimeInformationIsOsPlatform(OSPlatform.OSX) )
		{
			return false;
		}

		return new QuicklookMacOs(logger).GenerateThumbnail(filePath, outputPath, width, height);
	}

	/// <summary>
	///     Use to overwrite the RuntimeInformation.IsOSPlatform
	/// </summary>
	internal delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);
}
