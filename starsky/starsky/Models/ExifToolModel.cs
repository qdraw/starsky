using System;
using System.Collections.Generic;
using System.Linq;

namespace starsky.Models
{
    public class ExifToolModel
    {
        private FileIndexItem.Color _colorClass;

        public FileIndexItem.Color ColorClass
        {
            get { return _colorClass;}
            set { _colorClass = value; }
        }

        public string Prefs
        {
            get { return null; }
            set
            {
                // input: "Tagged:0, ColorClass:2, Rating:0, FrameNum:0"
                var prefsList = value.Split(", ");
                var firstColorClass = prefsList.FirstOrDefault(p => p.Contains("ColorClass"));
                if (firstColorClass != null)
                {
                    var stringColorClassItem = firstColorClass.Replace("ColorClass:", "");
                    _colorClass = new FileIndexItem().SetColorClass(stringColorClassItem);
                }
            }
        }

        private HashSet<string> _keywords;
        
        public HashSet<string> Keywords
        {
            get { return null; }
            set {
                    if (value == null)
                    {
                        _keywords = new HashSet<string>();
                        return;
                    }
                    _keywords = value; 
            }
        }

        public string Tags
        {
            get { return _hashSetToString(_keywords); }
            set { _keywords = _stringToHashSet(value); }
        }

        private static HashSet<string> _stringToHashSet(string inputKeywords)
        {
            HashSet<string> keywordsHashSet = inputKeywords.Split(", ").ToHashSet();
            return keywordsHashSet;
        }

        private static string _hashSetToString(HashSet<string> hashSetKeywords)
        {
            if (hashSetKeywords == null)
            {
                return string.Empty;
            }
            
            var toBeAddedKeywords = string.Empty;
            foreach (var keyword in hashSetKeywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword) && keyword != hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywords += keyword + ", ";
                }
                if (!string.IsNullOrWhiteSpace(keyword) && keyword == hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywords += keyword;
                }
            }
            // Add everyting in lowercase
            toBeAddedKeywords = toBeAddedKeywords.ToLower();

            return toBeAddedKeywords;
        }

    }
}
