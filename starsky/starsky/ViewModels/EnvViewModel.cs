using starsky.Models;

namespace starsky.ViewModels
{
    public class EnvViewModel
    {
        public  string DbConnectionString { get; set; }
        public  string BasePath { get; set; }
        public AppSettingsProvider.DatabaseTypeList DatabaseType { get; set; }
        public string ThumbnailTempFolder { get; set; }
        public string ExifToolPath { get; set; }
    }
}
