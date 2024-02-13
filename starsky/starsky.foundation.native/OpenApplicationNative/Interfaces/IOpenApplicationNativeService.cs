namespace starsky.foundation.native.OpenApplicationNative.Interfaces;

public interface IOpenApplicationNativeService
{
	bool? OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl);
}
