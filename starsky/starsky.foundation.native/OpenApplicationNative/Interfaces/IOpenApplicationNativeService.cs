namespace starsky.foundation.native.OpenApplicationNative.Interfaces;

public interface IOpenApplicationNativeService
{
	bool? OpenApplicationAtUrl(List<(string, string)> fullPathAndApplicationUrl);
	bool? OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl);
	bool? OpenDefault(List<string> fullPaths);
}
