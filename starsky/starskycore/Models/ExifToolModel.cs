using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace starskycore.Models
{
    public class ExifToolModel
    {
        
        public string SourceFile { get; set; }
        
        public FileIndexItem.Color ColorClass { get; set; }

        
        // Replace user Quoted input with single quote to avoid SQL Injection.
        private string _captionabstract;


        //[JsonProperty(PropertyName="Caption-Abstract")]
		[JsonPropertyName("Caption-Abstract")]
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
            get => null;
        }
        
        // Don't ignore this one, when setting it will also ignored
        public string Prefs
        {
            get { return null; }
            set
            {
                // input: "Tagged:0, ColorClass:2, Rating:0, FrameNum:0"
                var prefsList = value.Split(", ".ToCharArray());
                var firstColorClass = prefsList.FirstOrDefault(p => p.Contains("ColorClass"));
                if (firstColorClass != null)
                {
                    var stringColorClassItem = firstColorClass.Replace("ColorClass:", "");
                    ColorClass = new FileIndexItem().GetColorClass(stringColorClassItem);
                }
            }
        }

        private HashSet<string> _keywords = new HashSet<string>();
        
        public HashSet<string> Keywords
        {
            get => _keywords;
			// keep null? temp off
            set {
                    if (value != null)
                    {
                        _keywords = value; 
                    }
            }
        }

        public string Tags
        {
            get { return HashSetHelper.HashSetToString(_keywords); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _keywords = HashSetHelper.StringToHashSet(value);
                }
            }
        }

        // overwrite "-xmp:subject" over -Keywords
        public HashSet<string> Subject
        {
            set {
                if (_keywords.Count == 0)
                {
                    _keywords = value;
                }
            }
            get => null;
        }

        public string ObjectName { get; set; } = string.Empty;
        
        // overwrite "-xmp:title" over -ObjectName
        public string Title
        {
            set {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ObjectName = value;
                }
            }
            get => null;
        }
        

        public DateTime AllDatesDateTime { get; set; }
        
        //  Orientation   : 6
        public FileIndexItem.Rotation Orientation { get; set; }
        
        public ushort ImageWidth { get; set; }
        public ushort ImageHeight { get; set; }
    }
}
