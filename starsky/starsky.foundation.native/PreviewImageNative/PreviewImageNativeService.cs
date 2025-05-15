using System.Runtime.InteropServices;
using starsky.foundation.injection;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.native.PreviewImageNative;

[Service(typeof(IPreviewImageNativeService), InjectionLifetime = InjectionLifetime.Scoped)]
public class PreviewImageNativeService(IWebLogger logger) : IPreviewImageNativeService
{
	/// <summary>
	///     Get FileExtension without dot
	/// </summary>
	/// <returns>bmp or jpg</returns>
	public string FileExtension()
	{
		return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bmp" : "jpg";
	}

	/// <summary>
	///     Creates an image preview using the native QuickLook framework on macOS.
	/// </summary>
	/// <param name="filePath">where from</param>
	/// <param name="outputPath">to</param>
	/// <param name="width">in pixels</param>
	/// <param name="height">in pixels</param>
	/// <returns>is success</returns>
	public bool GeneratePreviewImage(string filePath, string outputPath, int width, int height)
	{
		return GeneratePreviewImage(RuntimeInformation.IsOSPlatform,
			filePath,
			outputPath,
			width,
			height);
	}

	public bool IsSupported(int width = 512)
	{
		return IsSupported(RuntimeInformation.IsOSPlatform, width);
	}

	private static bool IsSupported(IsOsPlatformDelegate runtimeInformationIsOsPlatform, int width)
	{
		return runtimeInformationIsOsPlatform(OSPlatform.OSX) ||
		       ( runtimeInformationIsOsPlatform(OSPlatform.Windows)
		         && RuntimeInformation.OSArchitecture == Architecture.X64 && width <= 512 );
	}

	internal bool GeneratePreviewImage(
		IsOsPlatformDelegate runtimeInformationIsOsPlatform,
		string filePath, string outputPath, int width, int height)
	{
		if ( runtimeInformationIsOsPlatform(OSPlatform.Linux) ||
		     runtimeInformationIsOsPlatform(OSPlatform.FreeBSD) )
		{
			return false;
		}

		if ( runtimeInformationIsOsPlatform(OSPlatform.OSX) )
		{
			return new QuicklookMacOs(logger).GenerateThumbnail(filePath, outputPath, width,
				height);
		}

		return new ShellThumbnailExtractionWindows(logger).GenerateThumbnail(filePath, outputPath,
			width,
			height);
	}

	/// <summary>
	///     Use to overwrite the RuntimeInformation.IsOSPlatform
	/// </summary>
	internal delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);
}
