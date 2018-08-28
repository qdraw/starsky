
using starsky.Services;

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
                if (_sourceMaxWith >= 2) return _sourceMaxWith;
                return 2;
            }
            set => _sourceMaxWith = value;
        }

        public int OverlayMaxWidth { get; set; }
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

        public string Append { get; set; }
        public string Template { get; set; } // index.cshtml for example
        public string Prepend { get; set; } = string.Empty;
    }

    public enum TemplateContentType
    {
        None = 0,
        Html = 1,
        Jpeg = 2,
        JpegBase64 = 3
    }
}