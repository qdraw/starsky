using System;
using System.IO;
using Newtonsoft.Json.Linq;
using starsky.Models;

namespace starsky.Services
{
    public static class ConfigRead
    {
        // Write the setting to the Model.AppSettingsProvider
        // Read settings first from appsettings.json + and later from ENV.
        public static void SetAppSettingsProvider()
        {
            // First read from env variables, if not read appsettings.json

            JObject obj = null;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json"))
            {
                string text =
                    File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json");
                obj = JObject.Parse(text);    
            }

            var basePath = _readTextFromObjOrEnv("STARSKY_BASEPATH", obj);
            var defaultConnection = _readTextFromObjOrEnv("DefaultConnection", obj);
            var databaseType =_readTextFromObjOrEnv("DatabaseType", obj);
            var thumbnailTempFolder =_readTextFromObjOrEnv("ThumbnailTempFolder", obj);
            thumbnailTempFolder = AddBackslash(thumbnailTempFolder);
            
            var exifToolPath = _readTextFromObjOrEnv("ExifToolPath", obj);

            // Read /.config.json
            /*
            {
                "readonly":  ["test","test"]
            }
            */

            if (File.Exists(Path.Combine(basePath, ".config.json")))
            {
                string text = File.ReadAllText(Path.Combine(basePath, ".config.json"));
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<BasePathConfig>(text);
                AppSettingsProvider.ReadOnlyFolders = model.Readonly;
            }


            AppSettingsProvider.BasePath = basePath;
            AppSettingsProvider.DatabaseType = databaseType == "mysql"
                ? AppSettingsProvider.DatabaseTypeList.Mysql
                : AppSettingsProvider.DatabaseTypeList.Sqlite;
            AppSettingsProvider.DbConnectionString = defaultConnection; // First database type
            AppSettingsProvider.ThumbnailTempFolder = thumbnailTempFolder;
            AppSettingsProvider.ExifToolPath = exifToolPath;

            if(AppSettingsProvider.Verbose) Console.WriteLine("DatabaseType: " +AppSettingsProvider.DatabaseType.ToString() );

        }

        private static string _readTextFromObjOrEnv(string name, JObject obj = null)
        {
            // input=text, nameofvar=text 
            // >>> Base Path of Orginal images <<<
            var value = Environment.GetEnvironmentVariable(name);

            // >>> Base Path of Orginal images <<<
            if(obj != null && IsSettingEmpty(value, name)) {
                value = (string)obj["ConnectionStrings"][name];
                IsSettingEmpty(value, name,true);
                value = RemoveLatestBackslash(value);
            }
            IsSettingEmpty(value, name,true);
            value = RemoveLatestBackslash(value);
            return value;
        }

        public static bool IsSettingEmpty(string setting, string name = "", bool throwError = false)
        {
            if (string.IsNullOrWhiteSpace(setting) && throwError) throw new FileNotFoundException(name + " ==null");
            if (string.IsNullOrWhiteSpace(setting)) return true;
            return false;
        }

        public static string RemoveLatestBackslash(string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath)) throw new FileNotFoundException("Error");

            if (basePath == "/") return basePath;
            
            // remove latest backslash
            if (basePath.Substring(basePath.Length - 1, 1) == Path.DirectorySeparatorChar.ToString())
            {
                basePath = basePath.Substring(0, basePath.Length - 1);
            }
            return basePath;
        }

        public static string AddBackslash(string thumbnailTempFolder) { 
            // Add backSlash to configuration
            if (thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1,
                1) != Path.DirectorySeparatorChar.ToString())
            {
                thumbnailTempFolder += Path.DirectorySeparatorChar.ToString();
            }

            return thumbnailTempFolder;
        }

    }
}
