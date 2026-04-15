using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.import.Helpers;

public static class RealPathHelper
{
	[DllImport("libc", SetLastError = true)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern IntPtr realpath(string path, IntPtr resolvedPath);

	[DllImport("libc")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	private static extern void free(IntPtr ptr);

	internal static string GetRealPath(string path)
	{
		if ( string.IsNullOrWhiteSpace(path) )
		{
			return path;
		}

		var result = realpath(path, IntPtr.Zero);
		if ( result == IntPtr.Zero )
		{
			return path;
		}

		try
		{
			return Marshal.PtrToStringAnsi(result) ?? path;
		}
		finally
		{
			free(result);
		}
	}
}
