using System;
using System.Runtime.InteropServices;

namespace starsky.foundation.platform.Trash;

/// <summary>
/// @see: https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin
/// @see: https://stackoverflow.com/a/17618
/// </summary>
public class WindowsShellTrashBindingHelper
{
	
	/// <summary>
	/// File Operation Function Type for SHFileOperation
	/// </summary>
	public enum FileOperationType : uint
	{
		/// <summary>
		/// Delete (or recycle) the objects
		/// </summary>
		FO_DELETE = 0x0003,
	}
	
	[Flags]
	public enum FileOperationFlags : ushort
	{
		/// <summary>
		/// Do not ask the user to confirm selection
		/// </summary>
		FOF_NOCONFIRMATION = 0x0010,
		/// <summary>
		/// Warn if files are too big to fit in the recycle bin and will need
		/// to be deleted completely.
		/// </summary>
		FOF_WANTNUKEWARNING = 0x4000,
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	public struct SHFILEOPSTRUCT
	{
		public IntPtr hwnd;
		[MarshalAs(UnmanagedType.U4)]
		public FileOperationType wFunc;
		public string pFrom;
		public string pTo;
		public FileOperationFlags fFlags;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;
		public IntPtr hNameMappings;
		public string lpszProgressTitle;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

	public static int? DeleteFileOperation(string filePath)
	{
		SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT();
		fileop.wFunc = FileOperationType.FO_DELETE;
		fileop.pFrom = filePath + '\0' + '\0';
		fileop.fFlags = FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING;

		try
		{
			return SHFileOperation(ref fileop);
		}
		catch ( Exception )
		{
			return null;
		}
	}

}
