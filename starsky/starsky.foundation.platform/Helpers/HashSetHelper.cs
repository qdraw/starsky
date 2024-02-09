using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers
{
	public static partial class HashSetHelper
	{
		/// <summary>
		/// Precompiled regex for splitting comma separated string
		/// Regex.Split
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			@",\s",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
			matchTimeoutMilliseconds: 200)]
		private static partial Regex DotCommaRegex();

		/// <summary>
		/// Split dot comma space string (used for tags) to unique list
		/// </summary>
		/// <param name="inputKeywords">comma seperated string</param>
		/// <returns>list/hashset with items in string</returns>
		public static HashSet<string> StringToHashSet(string inputKeywords)
		{
			if ( string.IsNullOrEmpty(inputKeywords) )
			{
				return [];
			}

			var keywords = ReplaceSingleCommaWithCommaWithSpace(inputKeywords);

			var keywordList = DotCommaRegex().Split(keywords);

			keywordList = TrimCommaInList(keywordList);

			// remove only leading and trailing whitespaces,
			keywordList = keywordList.Select(t => t.Trim()).ToArray();

			var keywordsHashSet = new HashSet<string>(from x in keywordList
													  select x);

			return keywordsHashSet;
		}


		/// <summary>
		/// To replace: 'test,fake' with 'test, fake' - Regex
		/// unescaped regex: (,(?=\S)|:)
		/// Precompiled Regex.Replace
		/// </summary>
		/// <returns>Regex object</returns>
		[GeneratedRegex(
			"(,(?=\\S)|:)",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
			matchTimeoutMilliseconds: 200)]
		private static partial Regex SingleCommaWithCommaWithSpaceRegex();

		/// <summary>
		/// To replace: 'test,fake' with 'test, fake'
		/// </summary>
		/// <param name="keywords">input string</param>
		/// <returns>comma separated string</returns>
		private static string ReplaceSingleCommaWithCommaWithSpace(string keywords)
		{
			return SingleCommaWithCommaWithSpaceRegex().Replace(keywords, ", ");
		}

		/// <summary>
		/// removing ,,,,before keyword to avoid
		/// testing with double commas those are not supported by the c# exif read tool
		/// </summary>
		/// <param name="keywordList"></param>
		/// <returns></returns>
		private static string[] TrimCommaInList(string[] keywordList)
		{
			for ( var i = 0; i < keywordList.Length; i++ )
			{
				var keyword = keywordList[i];

				char[] comma = [','];
				keyword = keyword.TrimEnd(comma);
				keyword = keyword.TrimStart(comma);

				keywordList[i] = keyword;
			}

			return keywordList;
		}


		/// <summary>
		/// Get a string with comma separated values from the hashset
		/// </summary>
		/// <param name="hashSetKeywords">import hashset</param>
		/// <returns>string with comma separated values</returns>
		public static string HashSetToString(HashSet<string>? hashSetKeywords)
		{
			return hashSetKeywords == null ? string.Empty : ListToString(hashSetKeywords.ToList());
		}

		/// <summary>
		/// Lists to string with comma separated values
		/// </summary>
		/// <param name="listKeywords">The list keywords</param>
		/// <returns></returns>
		internal static string ListToString(List<string>? listKeywords)
		{
			if ( listKeywords == null )
			{
				return string.Empty;
			}

			var toBeAddedKeywordsStringBuilder = new StringBuilder();
			foreach ( var keyword in listKeywords.Where(keyword =>
						 !string.IsNullOrWhiteSpace(keyword)) )
			{
				if ( keyword != listKeywords.LastOrDefault() )
				{
					toBeAddedKeywordsStringBuilder.Append(keyword + ", ");
					continue;
				}

				toBeAddedKeywordsStringBuilder.Append(keyword);
			}

			var toBeAddedKeywords = toBeAddedKeywordsStringBuilder.ToString();

			return toBeAddedKeywords;
		}
	}
}
