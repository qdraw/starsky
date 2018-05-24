using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace starsky.Models
{
    public class ExifToolModel
    {
        public FileIndexItem.Color ColorClass { get; set; }

        [JsonProperty(PropertyName="Caption-Abstract")]
        public string CaptionAbstract { get; set; }
        
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
                    ColorClass = new FileIndexItem().SetColorClass(stringColorClassItem);
                }
            }
        }

        private HashSet<string> _keywords;
        
        public HashSet<string> Keywords
        {
            get { return _keywords; } // keep null? temp off
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
            
            var toBeAddedKeywordsStringBuilder = new StringBuilder();
            foreach (var keyword in hashSetKeywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword) && keyword != hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywordsStringBuilder.Append(keyword + ", ");
                }
                if (!string.IsNullOrWhiteSpace(keyword) && keyword == hashSetKeywords.LastOrDefault())
                {
                    toBeAddedKeywordsStringBuilder.Append(keyword);
                }
            }
            var toBeAddedKeywords = toBeAddedKeywordsStringBuilder.ToString();

            return toBeAddedKeywords;
        }

    }
}
