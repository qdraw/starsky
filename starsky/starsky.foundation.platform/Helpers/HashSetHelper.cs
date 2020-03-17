using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers
{
	public static class HashSetHelper
	{
		/// <summary>
		/// Split dot comma space string (used for tags) to unique list
		/// </summary>
		/// <param name="inputKeywords">comma seperated string</param>
		/// <returns>list/hashset with items in string</returns>
		public static HashSet<string> StringToHashSet(string inputKeywords)
		{
			if ( string.IsNullOrEmpty(inputKeywords) ) return new HashSet<string>();
			
			var keywords = ReplaceSingleCommaWithCommaWithSpace(inputKeywords);

			var dotcommaRegex = new Regex(@",\s");

			var keywordList = dotcommaRegex.Split(keywords);

			keywordList = TrimCommaInList(keywordList);

			// remove only leading and trailing whitespaces,
			keywordList = keywordList.Select(t => t.Trim()).ToArray();

			HashSet<string> keywordsHashSet = new HashSet<string>(from x in keywordList
				select x);

			return keywordsHashSet;
		}

		/// <summary>
		/// To replace: test,fake with test, fake
		/// </summary>
		/// <param name="keywords">input string</param>
		/// <returns>comma separated string</returns>
		private static string ReplaceSingleCommaWithCommaWithSpace(string keywords)
		{
			// unescaped regex: (,(?=\S)|:)
			Regex pattern = new Regex("(,(?=\\S)|:)");
			keywords = pattern.Replace(keywords, ", ");
			return keywords;
		}

		/// <summary>
		/// removing ,,,,before keyword to avoid
		/// testing with double commas those are not supported by the c# exif read tool
		/// </summary>
		/// <param name="keywordList"></param>
		/// <returns></returns>
		private static string[] TrimCommaInList(string[] keywordList)
		{
			for ( int i = 0; i < keywordList.Length; i++ )
			{
				var keyword = keywordList[i];

				char[] comma = {','};
				keyword = keyword.TrimEnd(comma);
				keyword = keyword.TrimStart(comma);

				keywordList[i] = keyword;
			}

			return keywordList;
		}


		/// <summary>
		/// Get a string with comma seperated values from the hashset
		/// </summary>
		/// <param name="hashSetKeywords">import hashset</param>
		/// <returns>string with comma seperated values</returns>
		public static string HashSetToString(HashSet<string> hashSetKeywords)
		{
			if ( hashSetKeywords == null )
			{
				return string.Empty;
			}

			return ListToString(hashSetKeywords.ToList());
		}

		/// <summary>
		/// Lists to string with comma separated values
		/// </summary>
		/// <param name="listKeywords">The list keywords</param>
		/// <returns></returns>
		public static string ListToString(List<string> listKeywords)
		{

			if ( listKeywords == null )
			{
				return string.Empty;
			}

			var toBeAddedKeywordsStringBuilder = new StringBuilder();
			foreach ( var keyword in listKeywords.Where(keyword => !string.IsNullOrWhiteSpace(keyword)) )
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
