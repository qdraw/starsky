namespace starsky.foundation.native.PreviewImageNative.Interfaces;

public interface IPreviewImageNativeService
{
	/// <summary>
	///     Is Native supported?
	/// </summary>
	/// <returns></returns>
	bool IsSupported(int width = 512);

	string FileExtension();

	/// <summary>
	///     Creates an image preview using the native QuickLook framework on macOS.
	/// </summary>
	/// <param name="filePath">where from</param>
	/// <param name="outputPath">to</param>
	/// <param name="width">in pixels</param>
	/// <param name="height">in pixels</param>
	/// <returns>is success</returns>
	bool GeneratePreviewImage(string filePath, string outputPath, int width, int height);
}
