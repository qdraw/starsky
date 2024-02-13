namespace starsky.foundation.native.OpenApplicationNative.Interfaces;

public interface IOpenApplicationNativeService
{
	Task<bool?> OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl);
}
