using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers;

public static partial class GenerateSlugHelperStaticRegex
{
	/// <summary>
	/// Clean including _ and @ regex
	/// Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-_@]", // with _ & @
		RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 200)]
	private static partial Regex CleanIncludingLowercaseAndAtSignRegex();

	/// <summary>
	/// Clean including _ regex (no @)
	/// Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-_]", // no @
		RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 200)]
	private static partial Regex CleanIncludingLowercaseRegex();

	/// <summary>
	/// Clean including _ regex (no @)
	/// Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-@]", // no _
		RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 200)]
	private static partial Regex CleanIncludingAtRegex();

	/// <summary>
	/// Clean default regex (without _ and @)
	/// Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-]",
		RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 200)]
	private static partial Regex CleanDefaultRegex();

	public static string CleanReplaceInvalidCharacters(string text, bool allowAtSign, bool allowUnderScore)
	{
		switch ( allowAtSign )
		{
			case true when allowUnderScore:
				return CleanIncludingLowercaseAndAtSignRegex().Replace(text, string.Empty);
			case true:
				return CleanIncludingAtRegex().Replace(text, string.Empty);
		}

		if ( allowUnderScore )
		{
			return CleanIncludingLowercaseRegex().Replace(text, string.Empty);
		}

		return CleanDefaultRegex().Replace(text, string.Empty);
	}

	/// <summary>
	/// Space + regex
	/// Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"\s+",
		RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 200)]
	private static partial Regex SpacePlusRegex();

	/// <summary>
	/// Remove multiple spaces and trim spaces at begin and end
	/// </summary>
	/// <param name="text">input text</param>
	/// <returns>cleaned text</returns>
	public static string CleanSpace(string text)
	{
		return SpacePlusRegex().Replace(text, " ").Trim();
	}

	/// <summary>
	/// Space regex
	/// Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"\s",
		RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 200)]
	private static partial Regex SpaceRegex();

	/// <summary>
	/// Replace space with hyphen
	/// --- replace is for example: "test[space]-[space]test";
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static string ReplaceSpaceWithHyphen(string text)
	{
		return SpaceRegex()
			.Replace(text, "-")
			.Replace("---", "-");
	}
}
