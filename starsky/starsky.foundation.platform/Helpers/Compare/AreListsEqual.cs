using System.Collections.Generic;

namespace starsky.foundation.platform.Helpers.Compare;

public static class AreListsEqualHelper
{
	internal static bool AreListsEqual<T>(List<T> list1, List<T> list2)
	{
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
