using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

public class FileAssociation
{
	public string Extension { get; set; } = string.Empty;
	public string ProgId { get; set; } = string.Empty;
	public string FileTypeDescription { get; set; } = string.Empty;
	public string ExecutableFilePath { get; set; } = string.Empty;
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
	Justification = "Check build in")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Performance", "CA1806:Do not ignore method results")]
public static class WindowsSetFileAssociations
{
	/// <summary>
	/// needed so that Explorer windows get refreshed after the registry is updated
	/// https://stackoverflow.com/questions/2681878/associate-file-extension-with-application
	/// </summary>
	/// <param name="eventId"></param>
	/// <param name="flags"></param>
	/// <param name="item1"></param>
	/// <param name="item2"></param>
	/// <returns></returns>
	[System.Runtime.InteropServices.DllImport("Shell32.dll")]
	[SuppressMessage("Interoperability", "SYSLIB1054:Use \'LibraryImportAttribute\' " +
	                                     "instead of \'DllImportAttribute\' to generate P/Invoke " +
	                                     "marshalling code at compile time")]
	private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

	private const int SHCNE_ASSOCCHANGED = 0x8000000;
	private const int SHCNF_FLUSH = 0x1000;

	public static bool EnsureAssociationsSet(params FileAssociation[] associations)
	{
		var madeChanges = false;
		foreach ( var association in associations )
		{
			madeChanges |= SetAssociation(
				association.Extension,
				association.ProgId,
				association.FileTypeDescription,
				association.ExecutableFilePath);
		}

		if ( madeChanges )
		{
			SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
		}

		return madeChanges;
	}

	internal static bool SetAssociation(string extension, string progId, string fileTypeDescription,
		string applicationFilePath)
	{
		var madeChanges = false;
		madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
		madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
		madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command",
			"\"" + applicationFilePath + "\" \"%1\"");
		return madeChanges;
	}

	internal static bool SetKeyDefaultValue(string keyPath, string value)
	{
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			return false;
		}

		using var key = Registry.CurrentUser.CreateSubKey(keyPath);
		if ( key.GetValue(null) as string == value ) return false;
		key.SetValue(null, value);
		return true;
	}
}
