namespace starsky.foundation.native.OpenApplicationNative.Interfaces;

public interface IOpenApplicationNativeService
{
	/// <summary>
	/// Check if the system is supported to open a file
	/// Not all configurations are supported
	/// </summary>
	/// <returns>true is supported and false is not supported</returns>
	bool DetectToUseOpenApplication();
	
	/// <summary>
	/// Open with Default Editor
	/// Please check DetectToUseOpenApplication() before using this method
	/// </summary>
	/// <param name="fullPathAndApplicationUrl">List first item is fullFilePath, second is ApplicationUrl</param>
	/// <returns>open = true, null is unsupported</returns>
	bool? OpenApplicationAtUrl(List<(string fullFilePath, string applicationUrl)> fullPathAndApplicationUrl);
	
	/// <summary>
	/// Open with Default Editor
	/// Please check DetectToUseOpenApplication() before using this method
	/// </summary>
	/// <param name="fullPaths">Paths on disk</param>
	/// <returns>open = true, null is unsupported</returns>
	bool? OpenDefault(List<string> fullPaths);
}
