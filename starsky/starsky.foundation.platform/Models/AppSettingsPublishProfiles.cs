using System;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Helpers;
#if SYSTEM_TEXT_ENABLED
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
#endif

namespace starsky.foundation.platform.Models
{
    //"ContentType":  "html",
    //"SourceMaxWidth":  null,
    //"OverlayMaxWidth":  null,
    //"OverlayFullPath": null,
    //"Path": "index.html",
    //"Template": "index",
    //"Append": "_kl1k"
    
    public class AppSettingsPublishProfiles
    {
	  
	    /// <summary>
	    /// Type of template
	    /// </summary>
#if SYSTEM_TEXT_ENABLED
	    [JsonConverter(typeof(JsonStringEnumConverter))]
#else
	    [JsonConverter(typeof(StringEnumConverter))]
#endif
        public TemplateContentType ContentType { get; set; } = TemplateContentType.None;

        /// <summary>
        /// Private name of SourceMaxWidth
        /// </summary>
        private int _sourceMaxWidth;
        
        /// <summary>
        /// The size of the main image after resizing
        /// </summary>
        public int SourceMaxWidth
        {
            get
            {
                if (_sourceMaxWidth >= 100) return _sourceMaxWidth;
                return 100;
            }
            set => _sourceMaxWidth = value;
        }

        /// <summary>
        /// Private name for Overlay Image
        /// </summary>
        private int _overlayMaxWidth;
        
        /// <summary>
        /// Size of the overlay Image / logo
        /// </summary>
        public int OverlayMaxWidth
        {
            get
            {
                if (_overlayMaxWidth >= 100) return _overlayMaxWidth;
                return 100;
            }
            set => _overlayMaxWidth = value;
        }

	    
	    /// <summary>
	    /// private: used for template url or overlay image
	    /// </summary>
	    private string PathPrivate { get; set; } = string.Empty;

	    /// <summary>
	    /// used for template url or overlay image
	    /// </summary>
	    public string Path
	    {
		    get
		    {
			    // return: if null > string.Empty
			    return string.IsNullOrEmpty(PathPrivate) ? string.Empty : PathPrivate;
		    }
		    set
		    {
			    if ( string.IsNullOrEmpty(value) )
			    {
				    value = "{AssemblyDirectory}/WebHtmlPublish/EmbeddedViews/default.png";
			    }

			    if ( !value.Contains("{AssemblyDirectory}") )
			    {
				    PathPrivate = value;
				    return;
			    }
				// get current dir
			    var assemblyDirectory = PathHelper.RemoveLatestBackslash(AppDomain.CurrentDomain.BaseDirectory);
			    // replace value -- ignore this case
			    var subPath = Regex.Replace(value, "{AssemblyDirectory}", string.Empty, RegexOptions.IgnoreCase);
			    
			    // append and replace
			    PathPrivate = assemblyDirectory + subPath
				    .Replace("starskywebftpcli", "starskywebhtmlcli");
		    }
	    }

	    /// <summary>
	    /// Private Name for folder
	    /// </summary>
	    private string _folder = string.Empty;
	    
	    /// <summary>
	    /// To copy folder
	    /// </summary>
        public string Folder
        {
            get { return _folder; }
            set
            {
				// Append slash after
				if ( string.IsNullOrEmpty(value) )
				{
					_folder = PathHelper.AddSlash(string.Empty);
					return;
				}
	            _folder = PathHelper.AddSlash(value);
			}
		}

        /// <summary>
        /// do not add slash check, used for _kl
        /// </summary>
        public string Append { get; set; } = string.Empty;
       
        /// <summary>
        /// index.cshtml for example
        /// </summary>
        public string Template { get; set; }
        
        /// <summary>
        /// To add before
        /// </summary>
        public string Prepend { get; set; } = string.Empty;
        
        /// <summary>
        /// Include Exif Data
        /// </summary>
        public bool MetaData { get; set; } = true;

	    /// <summary>
	    /// For the ftp client to ignore some directories
	    /// </summary>
	    public bool Copy { get; set; } = true;

    }

    public enum TemplateContentType
    {
	    /// <summary>
	    /// Default, should pick one of the other options
	    /// </summary>
        None = 0,
        /// <summary>
        /// Generate Html lists
        /// </summary>
        Html = 1,
        /// <summary>
        /// Create a Jpeg Image
        /// </summary>
        Jpeg = 2,
        /// <summary>
        /// To move the source images to a folder, when using the web ui, this means copying 
        /// </summary>
        MoveSourceFiles = 3,
        /// <summary>
        /// Content to be copied from WebHtmlPublish/PublishedContent to include
        /// For example javaScript files
        /// </summary>
        PublishContent = 4,
        /// <summary>
        /// Include manifest file _settings.json in Copy list
        /// </summary>
        PublishManifest = 6,
    }
}
