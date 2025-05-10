namespace starsky.foundation.native.PreviewImageNative.Interfaces;

public interface IPreviewImageNativeService
{
	bool GeneratePreviewImage(string filePath, string outputPath, int width, int height);
}
