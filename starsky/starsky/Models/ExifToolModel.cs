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
        
        private string _keywords;

        public string Keywords
        {
            get { return _keywords; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _keywords = _duplicateKeywordCheck(value);
                }
                else
                {
                    _keywords = "";
                }
            }
        }
            
        private static string _duplicateKeywordCheck(string keywords)
        {
            var hashSetKeywords = new HashSet<string>(keywords.Split(", "));
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
