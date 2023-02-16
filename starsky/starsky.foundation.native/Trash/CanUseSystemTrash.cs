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
			var root = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly()?.Location)!;
			var (driveHasBin,_) = WindowsShellTrashBindingHelper.DriveHasRecycleBin(root);

			if ( driveHasBin == null ) return false;
			return (bool) driveHasBin;
		}
		
		return true; // yes is true
	}
}
