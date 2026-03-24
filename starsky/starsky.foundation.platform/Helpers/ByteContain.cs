using System;
using System.Text;

namespace starsky.foundation.platform.Helpers;

public static class ByteContain
{
	public static bool Contain(this byte[] bytes, string value)
	{
		if ( string.IsNullOrEmpty(value) )
		{
			return false;
		}

		var strBytes = Encoding.ASCII.GetBytes(value);

		// Early exit if search pattern is longer than the source
		if ( strBytes.Length > bytes.Length )
		{
			return false;
		}

		// Use MemoryExtensions for efficient byte sequence search
		return bytes.AsSpan().IndexOf(strBytes) >= 0;
	}
}
