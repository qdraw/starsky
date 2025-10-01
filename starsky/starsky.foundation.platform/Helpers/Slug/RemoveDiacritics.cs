using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace starsky.foundation.platform.Helpers.Slug;

internal static class ReplaceDiacritics
{
	private static readonly Dictionary<char, string> Replacements = new()
	{
		['Æ'] = "Ae",
		['æ'] = "ae",
		['Ø'] = "O",
		['ø'] = "o",
		['Ł'] = "L",
		['ł'] = "l",
		['ß'] = "ss",
		['Þ'] = "Th",
		['þ'] = "th",
		['Đ'] = "Dj",
		['đ'] = "dj",
		['Œ'] = "Oe",
		['œ'] = "oe"
	};

	internal static string ReplaceText(string text)
	{
		if ( string.IsNullOrEmpty(text) )
		{
			return text;
		}

		var replacedText = ReplaceNonStandardCharacters(text);
		return ReplaceDiacriticsFromLatinText(replacedText);
	}

	private static string ReplaceNonStandardCharacters(string text)
	{
		var sb = new StringBuilder(text.Length);
		foreach ( var c in text )
		{
			if ( Replacements.TryGetValue(c, out var replacement) )
			{
				sb.Append(replacement);
			}
			else
			{
				sb.Append(c);
			}
		}

		return sb.ToString();
	}

	private static string ReplaceDiacriticsFromLatinText(string text)
	{
		var normalized = text.Normalize(NormalizationForm.FormD);
		var chars = new char[normalized.Length];
		var idx = 0;
		foreach ( var c in normalized )
		{
			if ( CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark )
			{
				chars[idx++] = c;
			}
		}

		return new string(chars, 0, idx).Normalize(NormalizationForm.FormC);
	}
}
