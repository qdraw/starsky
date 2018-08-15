using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace starsky.Models
{
    public class ExifToolModel
    {
        public ExifToolModel(ExifToolModel model = null)
        {
            if (model == null) return;
            SourceFile = model.SourceFile;
            ColorClass = model.ColorClass;
            CaptionAbstract = model.CaptionAbstract;
            Keywords = model.Keywords;
            Orientation = model.Orientation;
            Status = model.Status;
        }
        
        public string SourceFile { get; set; }
        
        public FileIndexItem.Color ColorClass { get; set; }

        
        // Replace user Quoted input with single quote to avoid SQL Injection.
        private string _captionabstract;
        [JsonProperty(PropertyName="Caption-Abstract")]
        public string CaptionAbstract {
            get { 
                    return _captionabstract;
                }
            set
            {
                if(string.IsNullOrWhiteSpace(value)) return;
                _captionabstract = value.Replace("\"", "\'");
            } 
        }
        
        // overwrite "-xmp:Description" over -CaptionAbstract
        public string Description
        {
            set {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    CaptionAbstract = value;
                }
            }
        }
        
        // Don't ignore this one, when setting it will also ignored
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
                    ColorClass = new FileIndexItem().GetColorClass(stringColorClassItem);
                }
            }
        }

        private HashSet<string> keywords = new HashSet<string>();
        
        public HashSet<string> Keywords
        {
            get { return keywords; } // keep null? temp off
            set {
                    if (value != null)
                    {
                        keywords = value; 
                    }
            }
        }

        public string Tags
        {
            get { return hashSetToString(keywords); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    keywords = stringToHashSet(value);
                }
            }
        }

        // overwrite "-xmp:subject" over -Keywords
        public HashSet<string> Subject
        {
            set {
                if (keywords.Count == 0)
                {
                    keywords = value;
                }
            }
        }


        public DateTime AllDatesDateTime { get; set; }
        
        //  Orientation   : 6
        public FileIndexItem.Rotation Orientation { get; set; }
        
        
        public enum ExifStatus
        {
            NotFoundNotInIndex,
            NotFoundSourceMissing,
            ReadOnly,
            Ok
        }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ExifStatus Status { get; set; }


        private static HashSet<string> stringToHashSet(string inputKeywords)
        {
            HashSet<string> keywordsHashSet = inputKeywords.Split(", ").ToHashSet();
            return keywordsHashSet;
        }

        private static string hashSetToString(HashSet<string> hashSetKeywords)
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
