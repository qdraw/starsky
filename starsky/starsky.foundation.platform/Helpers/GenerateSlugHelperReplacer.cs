using System.Collections.Generic;
using System.Text;

namespace starsky.foundation.platform.Helpers;

public static class GenerateSlugHelperReplacer
{
	private static readonly Dictionary<char, string> Replacements = new()
	{
		{ 'ä', "a" },
		{ 'ë', "e" },
		{ 'é', "e" },
		{ 'ç', "c" },
		{ 'ü', "u" },
		{ 'ñ', "n" },
		{ 'ã', "a" },
		{ 'ô', "o" },
		{ 'ö', "o" },
		{ 'ß', "ss" },
		{ 'à', "a" },
		{ 'á', "a" },
		{ 'è', "e" },
		{ 'ê', "e" },
		{ 'ì', "i" },
		{ 'í', "i" },
		{ 'ò', "o" },
		{ 'ó', "o" },
		{ 'ù', "u" },
		{ 'ú', "u" },
		{ 'ý', "y" },
		{ 'ÿ', "y" },
		{ 'Æ', "ae" },
		{ 'æ', "ae" },
		{ 'Ø', "o" },
		{ 'ø', "o" },
		{ 'Å', "a" },
		{ 'å', "a" },
		{ 'ł', "l" },
		{ 'ž', "z" },
		{ 'š', "s" },
		{ 'č', "c" },
		{ 'đ', "d" },
		{ 'ğ', "g" },
		{ 'ı', "i" },
		{ 'ń', "n" },
		{ 'ř', "r" },
		{ 'ą', "a" },
		{ 'ę', "e" },
		{ 'œ', "oe" },
		{ 'þ', "th" },
		{ 'ð', "d" },
		{ 'ħ', "h" },
		{ '©', "(c)" },
		{ '®', "(r)" },
		{ '™', "(tm)" }
	};

	/// <summary>
	///     Replaces special characters in the input string
	///     for example: "België" becomes "belgie"
	/// </summary>
	/// <param name="input">with input</param>
	/// <returns>replaced value</returns>
	internal static string ReplaceSpecialCharacters(string input)
	{
		var result = new StringBuilder(input.Length);

		foreach ( var c in input )
		{
			result.Append(Replacements.TryGetValue(c, out var replacement)
				? replacement
				: c.ToString());
		}

		return result.ToString();
	}
}
