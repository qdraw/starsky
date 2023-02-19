using System.Runtime.InteropServices;
using starsky.foundation.injection;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.Trash.Helpers;
using starsky.foundation.native.Trash.Interfaces;

namespace starsky.foundation.native.Trash;

[Service(typeof(ITrashService), InjectionLifetime = InjectionLifetime.Scoped)]
public class TrashService : ITrashService
{
	/// <summary>
	/// Is the system trash supported
	/// </summary>
	/// <returns>true if supported, false if not supported</returns>
	public bool DetectToUseSystemTrash()
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
		
		return true;
	}

	/// <summary>
	/// Does not check if the file exists
	/// Trash file for Mac OS and Windows
	/// </summary>
	/// <param name="fullPath">system path</param>
	/// <returns>operation succeed (NOT if file is gone)</returns>
	public bool? Trash(string fullPath)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsTrash = MacOsTrashBindingHelper.Trash(fullPath, currentPlatform);
		var (windowsTrash,_) = WindowsShellTrashBindingHelper.Trash(fullPath, currentPlatform);
		return macOsTrash ?? windowsTrash;
	}
}
