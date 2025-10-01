namespace starsky.foundation.platform.Helpers.Slug;

public static class GenerateSlugHelper
{
	public const int MaxLength = 65;

	/// <summary>
	///     Generates a permalink slug for passed string
	/// </summary>
	/// <param name="phrase">input string</param>
	/// <param name="allowUnderScore">to allow underscores in slug</param>
	/// <param name="toLowerCase">change output to lowerCase</param>
	/// <param name="allowAtSign">allow @ (at sign) in name</param>
	/// <returns>clean slug string (ex. "some-cool-topic")</returns>
	public static string GenerateSlug(string phrase, bool allowUnderScore = false,
		bool toLowerCase = true, bool allowAtSign = false)
	{
		if ( string.IsNullOrEmpty(phrase) )
		{
			return phrase;
		}

		var text = toLowerCase ? phrase.ToLowerInvariant() : phrase;
		text = ReplaceDiacritics.ReplaceText(text);
		text = GenerateSlugHelperStaticRegex.CleanReplaceInvalidCharacters(text, allowAtSign,
			allowUnderScore);
		text = GenerateSlugHelperStaticRegex.CleanSpace(text);

		text = text[..( text.Length <= MaxLength ? text.Length : MaxLength )]
			.Trim(); // cut and trim
		text = GenerateSlugHelperStaticRegex.ReplaceSpaceWithHyphen(text);
		text = text.Trim('-'); // remove trailing hyphens
		return text;
	}
}
