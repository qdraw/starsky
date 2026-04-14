using starsky.foundation.native.FileSystem.Models;

namespace starsky.foundation.native.FileSystem.Interfaces;

public interface IMacOsNativeFilePicker
{
	MacOsFolderPickResult TryPickFolder(bool includeFiles = false);
}

