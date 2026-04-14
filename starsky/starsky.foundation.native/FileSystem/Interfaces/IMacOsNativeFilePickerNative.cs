namespace starsky.foundation.native.FileSystem.Interfaces;

internal interface IMacOsNativeFilePickerNative
{
	IntPtr CreateOpenPanel();
	// Returns a diagnostic message from the last native operation, if any
	string? GetLastNativeError();
	void ConfigureOpenPanel(IntPtr panel, bool includeFiles = false);
	bool RunModal(IntPtr panel);
	IntPtr GetSelectedUrl(IntPtr panel);
	string GetPath(IntPtr url);
	IntPtr CreateBookmarkData(IntPtr url);
	byte[] NsDataGetBytes(IntPtr nsData);
}

