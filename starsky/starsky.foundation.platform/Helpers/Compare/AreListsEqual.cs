using System;
using System.Collections.Generic;

namespace starsky.foundation.platform.Helpers.Compare;

public static class AreListsEqualHelper
{
	/// <summary>
	/// Compare two lists
	/// </summary>
	/// <param name="list1">First list</param>
	/// <param name="list2">Second list</param>
	/// <typeparam name="T">type of both lists</typeparam>
	/// <returns>true if same, false if not the same</returns>
	internal static bool AreListsEqual<T>(List<T> list1, List<T> list2)
	{
		ArgumentNullException.ThrowIfNull(list1);
		ArgumentNullException.ThrowIfNull(list2);

		if ( list1.Count != list2.Count )
		{
			return false;
		}

		for ( var i = 0; i < list1.Count; i++ )
		{
			if ( list1[i]?.Equals(list2[i]) == false )
			{
				return false;
			}
		}

		return true;
	}
}
