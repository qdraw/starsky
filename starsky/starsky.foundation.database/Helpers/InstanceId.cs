using System;
using starsky.foundation.platform.Extensions;

namespace starsky.foundation.database.Helpers;

public static class InstanceId
{
	/// <summary>
	/// Generates a new xmpMM:InstanceID value by using a supplied integer
	/// to create a unique identifier.
	/// </summary>
	/// <param name="numberForGuid">The integer value used to generate the instance ID.</param>
	/// <returns>A string representing the generated xmpMM:InstanceID value.</returns>
	public static string CreateNewInstanceId(int numberForGuid)
	{
		return $"xmp.iid:{numberForGuid.CreateGuid()}";
	}

	/// <summary>
	///     Generate a fresh xmpMM:InstanceID value.
	/// </summary>
	public static string CreateNewInstanceId()
	{
		return $"xmp.iid:{Guid.NewGuid():D}";
	}
}
