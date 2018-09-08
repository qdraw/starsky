using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace starsky.Helpers
{
    public class HashSetHelper
    {
        public static HashSet<string> StringToHashSet(string inputKeywords)
        {
            HashSet<string> keywordsHashSet = inputKeywords.Split(", ").ToHashSet();
            return keywordsHashSet;
        }

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