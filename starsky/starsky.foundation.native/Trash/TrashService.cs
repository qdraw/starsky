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
		return DetectToUseSystemTrashInternal(RuntimeInformation.IsOSPlatform, 
			Environment.UserInteractive, 
			Environment.UserName);
	}
	
	/// <summary>
	/// Use to overwrite the RuntimeInformation.IsOSPlatform
	/// </summary>
	internal delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);

	/// <summary>
	/// Is the system trash supported
	/// </summary>
	/// <param name="runtimeInformationIsOsPlatform">RuntimeInformation.IsOSPlatform</param>
	/// <param name="environmentUserInteractive">Environment.UserInteractive</param>
	/// <param name="environmentUserName">Environment.UserName</param>
	/// <returns>true if supported, false if not supported</returns>
	internal static bool DetectToUseSystemTrashInternal(IsOsPlatformDelegate runtimeInformationIsOsPlatform, 
		bool environmentUserInteractive, 
		string environmentUserName)
	{
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (runtimeInformationIsOsPlatform(OSPlatform.Linux) || 
		    runtimeInformationIsOsPlatform(OSPlatform.FreeBSD) || 
		    environmentUserName == "root" || !environmentUserInteractive )
		{
			return false;
		}

		// ReSharper disable once InvertIf
		if ( runtimeInformationIsOsPlatform(OSPlatform.Windows) )
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

	public bool? Trash(List<string> fullPaths)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsTrash = MacOsTrashBindingHelper.Trash(fullPaths, currentPlatform);
		var (windowsTrash,_) = WindowsShellTrashBindingHelper.Trash(fullPaths, currentPlatform);
		return macOsTrash ?? windowsTrash;
	}
}
