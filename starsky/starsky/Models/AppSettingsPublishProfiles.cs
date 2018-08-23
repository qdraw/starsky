
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
        public TemplateContentType ContentType { get; set; }

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
        public string Folder { get; set; }
        public string Append { get; set; }
        public string Template { get; set; } // index.cshtml for example
    }

    public enum TemplateContentType
    {
        None = 0,
        Html = 1,
        Jpeg = 2,
        JpegBase64 = 3
    }
}