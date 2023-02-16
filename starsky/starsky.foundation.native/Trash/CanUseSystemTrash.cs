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

		// ReSharper disable once InvertIf
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			var (driveHasBin,_,_) = WindowsShellTrashBindingHelper.DriveHasRecycleBin();
			return driveHasBin;
		}
		
		return true; // yes is true
	}
}
