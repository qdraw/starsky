namespace starsky.foundation.writemeta.Helpers;

public static class QuotesCommandLineEscapeHelper
{
	public static string QuotesCommandLineEscape(this string? input)
	{
		return input == null ? string.Empty : input.Replace("\"", "\\\"");
	}
}
