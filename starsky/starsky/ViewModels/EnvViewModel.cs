using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using starsky.Models;

namespace starsky.ViewModels
{
    public class EnvViewModel
    {
        [JsonProperty(PropertyName="DefaultConnection")]
        public  string DbConnectionString { get; set; }
        
        [JsonProperty(PropertyName="STARSKY_BASEPATH")]
        public  string BasePath { get; set; }
        
        [JsonProperty(PropertyName="DatabaseType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AppSettingsProvider.DatabaseTypeList DatabaseType { get; set; }

        [JsonProperty(PropertyName="ThumbnailTempFolder")]
        public string ThumbnailTempFolder { get; set; }
        
        [JsonProperty(PropertyName="ExifToolPath")]
        public string ExifToolPath { get; set; }
        
        [JsonProperty(PropertyName="AddMemoryCache")]
        public bool AddMemoryCache { get; set; }
        
        [JsonProperty(PropertyName="Structure")]
        public string Structure { get; set; }
        
        public List<string> ReadOnlyFolders { get; set; }

        public EnvViewModel GetEnvAppSettingsProvider()
        {
            var model = new EnvViewModel
            {
                DatabaseType = AppSettingsProvider.DatabaseType,
                BasePath = AppSettingsProvider.BasePath,
                ExifToolPath = AppSettingsProvider.ExifToolPath,
                ThumbnailTempFolder = AppSettingsProvider.ThumbnailTempFolder,
                AddMemoryCache = AppSettingsProvider.AddMemoryCache,
                Structure = AppSettingsProvider.Structure,
                ReadOnlyFolders = AppSettingsProvider.ReadOnlyFolders,
            };
            if (AppSettingsProvider.DatabaseType != AppSettingsProvider.DatabaseTypeList.mysql)
            {
                model.DbConnectionString = AppSettingsProvider.DbConnectionString;
            }

            return model;
        }

    }
}
