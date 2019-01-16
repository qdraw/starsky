
using starsky.Services;
using starskycore.Services;

namespace starsky.Models
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
        
        public string Path { get; set; } // used for template url or overlay image

        private string _folder = string.Empty;
        public string Folder
        {
            get { return _folder; }
            set
            {
                // Append slash after
                if (string.IsNullOrEmpty(value)) _folder = string.Empty;
                _folder = ConfigRead.AddSlash(value);
            }
        }

        public string Append { get; set; } = string.Empty; // do not add slash check, used for _kl
        public string Template { get; set; } // index.cshtml for example
        public string Prepend { get; set; } = string.Empty;
        public bool MetaData { get; set; } = true;

    }

    public enum TemplateContentType
    {
        None = 0,
        Html = 1,
        Jpeg = 2,
        MoveSourceFiles = 3
    }
}