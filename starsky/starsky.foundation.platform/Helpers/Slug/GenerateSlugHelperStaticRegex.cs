using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers.Slug;

internal static partial class GenerateSlugHelperStaticRegex
{
	/// <summary>
	///     Clean including _ and @ regular expression
	///     Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-_@]", // with _ & @
		RegexOptions.CultureInvariant,
		200)]
	private static partial Regex CleanIncludingLowercaseAndAtSignRegex();

	/// <summary>
	///     Clean including _ regex (no @)
	///     Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-_]", // no @
		RegexOptions.CultureInvariant,
		200)]
	private static partial Regex CleanIncludingLowercaseRegex();

	/// <summary>
	///     Clean including _ regex (no @)
	///     Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-@]", // no _
		RegexOptions.CultureInvariant,
		200)]
	private static partial Regex CleanIncludingAtRegex();

	/// <summary>
	///     Clean default regex (without _ and @)
	///     Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"[^a-zA-Z0-9\s-]",
		RegexOptions.CultureInvariant,
		200)]
	private static partial Regex CleanDefaultRegex();

	internal static string CleanReplaceInvalidCharacters(string text, bool allowAtSign,
		bool allowUnderScore)
	{
		return allowAtSign switch
		{
			true when allowUnderScore => CleanIncludingLowercaseAndAtSignRegex()
				.Replace(text, string.Empty),
			true => CleanIncludingAtRegex().Replace(text, string.Empty),
			_ => allowUnderScore
				? CleanIncludingLowercaseRegex().Replace(text, string.Empty)
				: CleanDefaultRegex().Replace(text, string.Empty)
		};
	}

	/// <summary>
	///     Space + regex
	///     Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"\s+",
		RegexOptions.CultureInvariant,
		200)]
	private static partial Regex SpacePlusRegex();

	/// <summary>
	///     Remove multiple spaces and trim spaces at begin and end
	/// </summary>
	/// <param name="text">input text</param>
	/// <returns>cleaned text</returns>
	internal static string CleanSpace(string text)
	{
		return SpacePlusRegex().Replace(text, " ").Trim();
	}

	/// <summary>
	///     Space regex
	///     Regex.Replace (pre compiled regex)
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"\s",
		RegexOptions.CultureInvariant,
		200)]
	private static partial Regex SpaceRegex();

	/// <summary>
	///     Replace space with hyphen
	///     --- replace is for example: "test[space]-[space]test";
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	internal static string ReplaceSpaceWithHyphen(string text)
	{
		return SpaceRegex()
			.Replace(text, "-")
			.Replace("---", "-");
	}
}
