using System;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Helpers;

namespace starskycore.Models
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
	    private string _path { get; set; } = string.Empty;

	    /// <summary>
	    /// used for template url or overlay image
	    /// </summary>
	    public string Path
	    {
		    get
		    {
			    // return: if null > string.Empty
			    return string.IsNullOrEmpty(_path) ? string.Empty : _path;
		    }
		    set
		    {
			    if (string.IsNullOrEmpty(value)) return;

			    if ( !value.Contains("{AssemblyDirectory}") )
			    {
				    _path = value;
				    return;
			    }
				// get current dir
			    var assemblyDirectory = PathHelper.RemoveLatestBackslash(AppDomain.CurrentDomain.BaseDirectory);
			    // replace value -- ignore this case
			    var subPath = Regex.Replace(value, "{AssemblyDirectory}", string.Empty, RegexOptions.IgnoreCase);
			    
			    // append and replace
			    _path = assemblyDirectory + subPath
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
        MoveSourceFiles = 3
    }
}
