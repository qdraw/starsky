using System;
using starsky.foundation.native.FileSystem.Interfaces;

namespace starskytest.FakeMocks;

public class FakeMacOsNativeFilePickerNative : IMacOsNativeFilePickerNative
{
	public IntPtr PanelToReturn { get; set; } = new(100);
	public bool RunModalResult { get; set; } = true;
	public IntPtr SelectedUrlToReturn { get; set; } = new(200);
	public string PathToReturn { get; set; } = "/tmp/selected-folder";
	public IntPtr BookmarkDataToReturn { get; set; } = new(300);
	public byte[] NsDataBytesToReturn { get; set; } = [1, 2, 3, 4];

	public bool ConfigureCalled { get; private set; }
	public bool LastIncludeFilesValue { get; private set; }

	public virtual IntPtr CreateOpenPanel()
	{
		return PanelToReturn;
	}

	public virtual void ConfigureOpenPanel(IntPtr panel, bool includeFiles = false)
	{
		ConfigureCalled = true;
		LastIncludeFilesValue = includeFiles;
	}

	public virtual bool RunModal(IntPtr panel)
	{
		return RunModalResult;
	}

	public virtual IntPtr GetSelectedUrl(IntPtr panel)
	{
		return SelectedUrlToReturn;
	}

	public virtual string GetPath(IntPtr url)
	{
		return PathToReturn;
	}

	public virtual IntPtr CreateBookmarkData(IntPtr url)
	{
		return BookmarkDataToReturn;
	}

	public virtual byte[] NsDataGetBytes(IntPtr nsData)
	{
		return NsDataBytesToReturn;
	}
}

