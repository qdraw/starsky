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
	    
#if SYSTEM_TEXT_ENABLED
	    [JsonConverter(typeof(JsonStringEnumConverter))]
#else
	    [JsonConverter(typeof(StringEnumConverter))]
#endif
        public TemplateContentType ContentType { get; set; } = TemplateContentType.None;

        private int _sourceMaxWith;
        public int SourceMaxWidth
        {
            get
            {
                if (_sourceMaxWith >= 100) return _sourceMaxWith;
                return 100;
            }
            set => _sourceMaxWith = value;
        }

        private int _overlayMaxWidth;
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
	    private string pathPrivate { get; set; } = string.Empty;

	    /// <summary>
	    /// used for template url or overlay image
	    /// </summary>
	    public string Path
	    {
		    get
		    {
			    // return: if null > string.Empty
			    return string.IsNullOrEmpty(pathPrivate) ? string.Empty : pathPrivate;
		    }
		    set
		    {
			    if ( string.IsNullOrEmpty(value) )
			    {
				    value = "{AssemblyDirectory}/WebHtmlPublish/EmbeddedViews/default.png";
			    }

			    if ( !value.Contains("{AssemblyDirectory}") )
			    {
				    pathPrivate = value;
				    return;
			    }
				// get current dir
			    var assemblyDirectory = PathHelper.RemoveLatestBackslash(AppDomain.CurrentDomain.BaseDirectory);
			    // replace value -- ignore this case
			    var subPath = Regex.Replace(value, "{AssemblyDirectory}", string.Empty, RegexOptions.IgnoreCase);
			    
			    // append and replace
			    pathPrivate = assemblyDirectory + subPath
				    .Replace("starskywebftpcli", "starskywebhtmlcli");
		    }
	    }

	    private string _folder = string.Empty;
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

        public string Append { get; set; } = string.Empty; // do not add slash check, used for _kl
        public string Template { get; set; } // index.cshtml for example
        public string Prepend { get; set; } = string.Empty;
        public bool MetaData { get; set; } = true;

	    /// <summary>
	    /// For the ftp client to ignore some directories
	    /// </summary>
	    public bool Copy { get; set; } = true;

    }

    public enum TemplateContentType
    {
        None = 0,
        Html = 1,
        Jpeg = 2,
        MoveSourceFiles = 3,
        /// <summary>
        /// Content to be copied from WebHtmlPublish/PublishedContent to include
        /// For example javaScript files
        /// </summary>
        PublishContent = 4,
        /// <summary>
        /// Include manifest file _settings.json in Copy list
        /// </summary>
        PublishManifest = 6
    }
}
