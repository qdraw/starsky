using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace starsky.Helpers
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
            
            var dotcommaRegex = new System.Text.RegularExpressions.Regex(", ");
            
            var keywordList = dotcommaRegex.Split(inputKeywords);

	        keywordList = TrimCommaInList(keywordList);
	        
            HashSet<string> keywordsHashSet = new HashSet<string>(from x in keywordList select x);

            return keywordsHashSet;
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
            if (hashSetKeywords == null)
            {
                return String.Empty;
            }
            
            var toBeAddedKeywordsStringBuilder = new StringBuilder();
            foreach (var keyword in hashSetKeywords)
            {
                if (String.IsNullOrWhiteSpace(keyword)) continue;
                
                if (!String.IsNullOrWhiteSpace(keyword) && keyword != hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywordsStringBuilder.Append(keyword + ", ");
                }
                if (!String.IsNullOrWhiteSpace(keyword) && keyword == hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywordsStringBuilder.Append(keyword);
                }
            }
            var toBeAddedKeywords = toBeAddedKeywordsStringBuilder.ToString();

            return toBeAddedKeywords;
        }
    }
}