using starsky.foundation.platform.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace starsky.foundation.platform.Trash;

/// <summary>
/// @see: https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin
/// @see: https://stackoverflow.com/a/17618
/// </summary>
public class WindowsShellTrashBindingHelper
{
	private IWebLogger _logger;

	public WindowsShellTrashBindingHelper(IWebLogger logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// Possible flags for the SHFileOperation method.
	/// </summary>
	[Flags]
	public enum FileOperationFlags : ushort
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
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct SHFILEOPSTRUCT
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
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

	/// <summary>
	/// Send file to recycle bin
	/// </summary>
	/// <param name="path">Location of directory or file to recycle</param>
	/// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
	public (bool, string) Send(string path, FileOperationFlags flags)
	{
		try
		{
			var fs = new SHFILEOPSTRUCT
			{
				wFunc = FileOperationType.FO_DELETE,
				pFrom = path + '\0' + '\0',
				fFlags = FileOperationFlags.FOF_ALLOWUNDO | flags
			};
			var result = SHFileOperation(ref fs).ToString();
			return (true, result);

		}
		catch ( Exception ex)
		{
			_logger.LogError(ex.Message, ex);
			return (false, ex.Message);
		}
	}

	/// <summary>
	/// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
	/// </summary>
	/// <param name="path">Location of directory or file to recycle</param>
	public (bool, string) Send(string path)
	{
		return Send(path, FileOperationFlags.FOF_NOCONFIRMATION |
			FileOperationFlags.FOF_WANTNUKEWARNING);
	}

}
