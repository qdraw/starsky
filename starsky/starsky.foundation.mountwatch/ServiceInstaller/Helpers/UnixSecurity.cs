using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

public class UnixSecurity
{
	[DllImport("libc")]
	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use \'LibraryImportAttribute\' " +
		"instead of \'DllImportAttribute\' to " +
		"generate P/Invoke marshalling code at compile time")]
	private static extern uint geteuid();

	/// <summary>
	///     Return effective user id. Extracted to a protected virtual method
	///     to make the class testable (allows overriding in unit tests).
	/// </summary>
	protected virtual uint GetEuid()
	{
		return geteuid();
	}

	public virtual bool IsRunningAsRoot()
	{
		return GetEuid() == 0;
	}
}
