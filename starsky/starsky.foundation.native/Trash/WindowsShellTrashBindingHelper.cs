using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.native.Trash;

/// <summary>
/// @see: https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin
/// @see: https://stackoverflow.com/a/17618
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Usage", "S101: Rename struct 'SHFILEOPSTRUCT' to match pascal case naming rules, consider using 'Shfileopstruct'.")]
public class WindowsShellTrashBindingHelper
{

	/// <summary>
	/// Possible flags for the SHFileOperation method.
	/// </summary>
	[Flags]
	public enum ShFileOperations : ushort
	{
		/// <summary>
		/// Do not show a dialog during the process
		/// </summary>
		FOF_SILENT = 0x0004,
		/// <summary>
		/// Do not ask the user to confirm selection
		/// </summary>
		FOF_NOCONFIRMATION = 0x0010,
		/// <summary>
		/// Delete the file to the recycle bin.  (Required flag to send a file to the bin
		/// </summary>
		FOF_ALLOWUNDO = 0x0040,
		/// <summary>
		/// Do not show the names of the files or folders that are being recycled.
		/// </summary>
		FOF_SIMPLEPROGRESS = 0x0100,
		/// <summary>
		/// Surpress errors, if any occur during the process.
		/// </summary>
		FOF_NOERRORUI = 0x0400,
		/// <summary>
		/// Warn if files are too big to fit in the recycle bin and will need
		/// to be deleted completely.
		/// </summary>
		FOF_WANTNUKEWARNING = 0x4000,
	}

	/// <summary>
	/// File Operation Function Type for SHFileOperation
	/// @see: https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-shfileopstructa
	/// SHFILEOPSTRUCTA
	/// </summary>
	public enum FileOperationType : uint
	{
		/// <summary>
		/// Move the objects
		/// </summary>
		FO_MOVE = 0x0001,
		/// <summary>
		/// Copy the objects
		/// </summary>
		FO_COPY = 0x0002,
		/// <summary>
		/// Delete (or recycle) the objects
		/// </summary>
		FO_DELETE = 0x0003,
		/// <summary>
		/// Rename the object(s)
		/// </summary>
		FO_RENAME = 0x0004,
	}

	/// <summary>
	/// SHFILEOPSTRUCT for SHFileOperation from COM
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct SHFILEOPSTRUCT
	{
		public IntPtr hwnd;
		[MarshalAs(UnmanagedType.U4)]
		public FileOperationType wFunc;
		public string pFrom;
		public string pTo;
		public ShFileOperations fFlags;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;
		public IntPtr hNameMappings;
		public string lpszProgressTitle;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

	/// <summary>
	/// Send file to recycle bin
	/// </summary>
	/// <param name="path">Location of directory or file to recycle</param>
	/// <param name="platform">should be windows</param>
	/// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
	public static (bool?, string) Trash(string path, 
		OSPlatform platform, 
		ShFileOperations flags = ShFileOperations.FOF_NOCONFIRMATION |
		                           ShFileOperations.FOF_WANTNUKEWARNING)
	{
		if ( platform != OSPlatform.Windows )
		{
			return (null, "Not supported on this platform");
		}

		return TrashInternal(path, flags);
	}

	public static (bool, string) TrashInternal(string path, ShFileOperations flags =
		ShFileOperations.FOF_NOCONFIRMATION |
		ShFileOperations.FOF_WANTNUKEWARNING)
	{
		try
		{
			var fs = new SHFILEOPSTRUCT
			{
				wFunc = FileOperationType.FO_DELETE,
				pFrom = path + '\0' + '\0',
				fFlags = ShFileOperations.FOF_ALLOWUNDO | flags
			};
			var result = SHFileOperation(ref fs).ToString();
			return (true, result);
		}
		catch ( Exception ex)
		{
			return (false, ex.Message);
		}
	}
	
	
	// @see: https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shgetknownfolderitem
	
	
	/// <summary>
	/// @see: https://stackoverflow.com/questions/7718028/how-do-i-detect-if-a-drive-has-a-recycle-bin-in-c
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	private struct SHQUERYRBINFO
	{
		/// DWORD->unsigned int
		public uint cbSize;

		/// __int64
		public long i64Size;

		/// __int64
		public long i64NumItems;
	}


	/// <summary>
	/// Return Type: HRESULT->LONG->int
	/// pszRootPath: LPCTSTR->LPCWSTR->WCHAR*
	/// pSHQueryRBInfo: LPSHQUERYRBINFO->_SHQUERYRBINFO*
	/// </summary>
	/// <param name="pszRootPath"></param>
	/// <param name="pSHQueryRBInfo"></param>
	/// <returns></returns>
	[DllImport("shell32.dll", EntryPoint = "SHQueryRecycleBinW")]
	private static extern int SHQueryRecycleBinW([In] [MarshalAs(UnmanagedType.LPWStr)] string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

	public static (bool?, string) DriveHasRecycleBin(string Drive)
	{
		var Info = new SHQUERYRBINFO { 
			cbSize = 20  //sizeof(SHQUERYRBINFO)
		};
		try
		{
			return (SHQueryRecycleBinW(Drive, ref Info) == 0, "Ok");
		}
		catch ( Exception e )
		{
			return ( null, e.Message );
		}
	}
	
}
