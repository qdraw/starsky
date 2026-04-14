namespace starsky.foundation.native.FileSystem.Interfaces;

internal interface IMacOsNativeFilePickerNative
{
	IntPtr CreateOpenPanel();
	void ConfigureOpenPanel(IntPtr panel, bool includeFiles = false);
	bool RunModal(IntPtr panel);
	IntPtr GetSelectedUrl(IntPtr panel);
	string GetPath(IntPtr url);
	IntPtr CreateBookmarkData(IntPtr url);
	byte[] NsDataGetBytes(IntPtr nsData);
}

