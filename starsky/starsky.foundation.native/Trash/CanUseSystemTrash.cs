using System.Runtime.InteropServices;

namespace starsky.foundation.native.Trash;

public static class CanUseSystemTrash
{
	public static bool UseTrash()
	{
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
		    RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) || 
		    Environment.UserName == "root" || !Environment.UserInteractive )
		{
			return false;
		}

		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			var root = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly()?.Location)!;
			return WindowsShellTrashBindingHelper.DriveHasRecycleBin(root);
		}
		
		return true; // yes is true
	}
}
