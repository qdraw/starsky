using System;

namespace starsky.foundation.platform.Extensions;

public static class IntegerExtensions
{
	/// <summary>
	/// Creates a Guid representation of the provided integer value by invoking
	/// an internal conversion method.
	/// </summary>
	/// <param name="i">The integer value to convert into a Guid.</param>
	/// <returns>A Guid representation of the provided integer value.</returns>
	public static Guid CreateGuid(this int i)
	{
		return IntegerToGuid(i);
	}

	/// <summary>
	/// Converts an integer value to a Guid, ensuring the integer is appropriately
	/// formatted to meet Guid requirements.
	/// </summary>
	/// <param name="value">The integer value to convert to a Guid.</param>
	/// <returns>A Guid representation of the provided integer value.</returns>
	private static Guid IntegerToGuid(int value)
	{
		return Guid.Parse(value.ToString("D32").PadLeft(32, '0'));
	}
}
