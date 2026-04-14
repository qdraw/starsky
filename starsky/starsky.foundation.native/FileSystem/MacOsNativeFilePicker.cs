using starsky.foundation.injection;
using starsky.foundation.native.FileSystem.Interfaces;
using starsky.foundation.native.FileSystem.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.native.FileSystem;

/// <summary>
/// Uses macOS NSOpenPanel to pick a folder and create a security-scoped bookmark token.
/// </summary>
[Service(typeof(IMacOsNativeFilePicker), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MacOsNativeFilePicker : IMacOsNativeFilePicker
{
	private readonly IWebLogger _logger;
	private readonly IMacOsNativeFilePickerNative _native;

	public MacOsNativeFilePicker(IWebLogger logger) : this(
		new MacOsNativeFilePickerNative(), logger)
	{
		_logger = logger;
	}

	internal MacOsNativeFilePicker(IMacOsNativeFilePickerNative native, IWebLogger logger)
	{
		_native = native;
		_logger = logger;
	}

	public MacOsFolderPickResult TryPickFolder(bool includeFiles = false)
	{
		var result = new MacOsFolderPickResult { Success = false };

		if ( !OperatingSystem.IsMacOS() )
		{
			result.Error = "macOS only";
			return result;
		}

		try
		{
			var panel = _native.CreateOpenPanel();
			if ( panel == IntPtr.Zero )
			{
				result.Error = "Could not create NSOpenPanel";
				_logger.LogError($"[MacOsNativeFilePicker] {result.Error}");
				return result;
			}

			_native.ConfigureOpenPanel(panel, includeFiles);

			if ( !_native.RunModal(panel) )
			{
				result.Error = "cancelled";
				return result;
			}

			var selectedUrl = _native.GetSelectedUrl(panel);
			if ( selectedUrl == IntPtr.Zero )
			{
				result.Error = "No URL returned by NSOpenPanel";
				_logger.LogError($"[MacOsNativeFilePicker] {result.Error}");
				return result;
			}

			var selectedPath = _native.GetPath(selectedUrl);
			if ( string.IsNullOrWhiteSpace(selectedPath) )
			{
				result.Error = "Could not resolve selected path";
				_logger.LogError($"[MacOsNativeFilePicker] {result.Error}");
				return result;
			}

			var bookmarkNsData = _native.CreateBookmarkData(selectedUrl);
			if ( bookmarkNsData == IntPtr.Zero )
			{
				result.Error = "Could not create bookmark data";
				_logger.LogError($"[MacOsNativeFilePicker] {result.Error}");
				return result;
			}

			var bytes = _native.NsDataGetBytes(bookmarkNsData);
			if ( bytes.Length == 0 )
			{
				result.Error = "Bookmark data is empty";
				_logger.LogError($"[MacOsNativeFilePicker] {result.Error}");
				return result;
			}

			result.Path = selectedPath;
			result.BookmarkToken = Convert.ToBase64String(bytes);
			result.Success = true;
			result.Error = null;
			return result;
		}
		catch
		{
			result.Error = "Exception while picking folder";
			return result;
		}
	}
}

