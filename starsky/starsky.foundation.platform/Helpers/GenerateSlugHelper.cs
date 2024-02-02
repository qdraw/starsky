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
		text = GenerateSlugHelperStaticRegex.CleanReplaceInvalidCharacters(text, allowAtSign, allowUnderScore);
		text = GenerateSlugHelperStaticRegex.CleanSpace(text);
		
		text = text.Substring(0, text.Length <= 65 ? text.Length : 65).Trim(); // cut and trim
		text = GenerateSlugHelperStaticRegex.ReplaceSpaceWithHyphen(text);
		text = text.Trim('-'); // remove trailing hyphens
		return text;
	}
}
