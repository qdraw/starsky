namespace starsky.foundation.database.Extensions;

public static class TruncateExtensions
{
	public static string TruncateWithEllipsis(this string value, int maxLength)
	{
		if ( maxLength < 3 )
		{
			return value.Length <= maxLength ? value : value[..maxLength];
		}

		return value.Length <= maxLength
			? value
			: value[..( maxLength - 3 )] + "...";
	}
}
