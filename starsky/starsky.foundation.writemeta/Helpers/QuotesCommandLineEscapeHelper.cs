using starsky.foundation.platform.Models;

namespace starsky.foundation.writemeta.Helpers;

public static class QuotesCommandLineEscapeHelper
{
	public static string QuotesCommandLineEscape(this string? input)
	{
		if ( input == null )
		{
			return string.Empty;
		}

		return new AppSettings().IsWindows
			? input.Replace("\"", "\"\"")
			: input.Replace("\"", "\\\"");
	}
}
