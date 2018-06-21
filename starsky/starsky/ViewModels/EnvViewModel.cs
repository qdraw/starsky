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

        public EnvViewModel GetEnvAppSettingsProvider()
        {
            var model = new EnvViewModel
            {
                DatabaseType = AppSettingsProvider.DatabaseType,
                BasePath = AppSettingsProvider.BasePath,
                ExifToolPath = AppSettingsProvider.ExifToolPath,
                ThumbnailTempFolder = AppSettingsProvider.ThumbnailTempFolder,
            };
            if (AppSettingsProvider.DatabaseType != AppSettingsProvider.DatabaseTypeList.mysql)
            {
                model.DbConnectionString = AppSettingsProvider.DbConnectionString;
            }

            return model;
        }
    }
}
