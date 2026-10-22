using System.Text;

namespace starsky.foundation.writemeta.Helpers;

public static class QuotesCommandLineEscapeHelper
{
	/// <summary>
	///     Escape a value that is placed between double quotes in a command line string.
	///     The arguments are split with the Windows rules, so backslashes in front of a quote
	///     (or at the end of the value, in front of the closing quote) must be doubled,
	///     otherwise a value like <c>x\" -config file</c> closes the quote and adds switches.
	/// </summary>
	public static string QuotesCommandLineEscape(this string? input)
	{
		if ( string.IsNullOrEmpty(input) )
		{
			return string.Empty;
		}

		var result = new StringBuilder(input.Length + 8);
		var backslashes = 0;
		foreach ( var c in input )
		{
			switch ( c )
			{
				case '\\':
					backslashes++;
					continue;
				case '"':
					// double the backslashes, then escape the quote itself
					result.Append('\\', backslashes * 2 + 1).Append('"');
					break;
				default:
					result.Append('\\', backslashes).Append(c);
					break;
			}

			backslashes = 0;
		}

		// backslashes before the closing quote of the argument
		result.Append('\\', backslashes * 2);
		return result.ToString();
	}
}
