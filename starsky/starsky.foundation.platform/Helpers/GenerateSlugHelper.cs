using System;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers;

public static class GenerateSlugHelper
{
	/// <summary>
	/// Generates a permalink slug for passed string
	/// </summary>
	/// <param name="phrase">input string</param>
	/// <param name="allowUnderScore">to allow underscores in slug</param>
	/// <param name="toLowerCase">change output to lowerCase</param>
	/// <param name="allowAtSign">allow @ (at sign) in name</param>
	/// <returns>clean slug string (ex. "some-cool-topic")</returns>
	public static string GenerateSlug(string phrase, bool allowUnderScore = false,
		bool toLowerCase = true, bool allowAtSign = false)
	{
		var text = toLowerCase ? phrase.ToLowerInvariant() : phrase;
		var regexTimespan = TimeSpan.FromMilliseconds(100);

		var charAllowLowerCase = allowUnderScore ? "_" : string.Empty;
		var charAllowAtSign = allowAtSign ? "@" : string.Empty;

		var matchNotRegexString = @"[^a-zA-Z0-9\s-" + charAllowLowerCase + charAllowAtSign + "]";

		text = Regex.Replace(text, matchNotRegexString, string.Empty,
			RegexOptions.None, regexTimespan);
		//						^^^ remove invalid characters
		text = Regex.Replace(text, @"\s+", " ",
			RegexOptions.None, regexTimespan).Trim(); // single space
		text = text.Substring(0, text.Length <= 65 ? text.Length : 65).Trim(); // cut and trim
		text = Regex.Replace(text, @"\s", "-", RegexOptions.None,
			regexTimespan); // insert hyphens
		text = text.Replace("---", "-"); // for example: "test[space]-[space]test"
		text = text.Trim('-'); // remove trailing hyphens
		return text;
	}
}
