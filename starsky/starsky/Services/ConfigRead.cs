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
            var appsettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(appsettingsFile))
            {
                string text =
                    File.ReadAllText(appsettingsFile);
                obj = JObject.Parse(text);    
            }

            if (obj == null) Console.WriteLine("> " + appsettingsFile + " is missing; \n you could use env variables");

            var basePath = ReadTextFromObjOrEnv("STARSKY_BASEPATH", obj);
            var defaultConnection = ReadTextFromObjOrEnv("DefaultConnection", obj);
            var databaseType = ReadTextFromObjOrEnv("DatabaseType", obj);
            var thumbnailTempFolder = ReadTextFromObjOrEnv("ThumbnailTempFolder", obj);
            var exifToolPath = ReadTextFromObjOrEnv("ExifToolPath", obj);

            SetAppSettingsProvider(basePath,defaultConnection,databaseType,thumbnailTempFolder,exifToolPath);
        }

        public static void SetAppSettingsProvider(string basePath,string defaultConnection,string databaseType,string thumbnailTempFolder, string exifToolPath)
        {

            thumbnailTempFolder = AddBackslash(thumbnailTempFolder);
            basePath = AddBackslash(basePath);

            // Read /.config.json
            // Please check the config example in the starsky folder

            if (File.Exists(Path.Combine(basePath, ".config.json")))
            {
                string text = File.ReadAllText(Path.Combine(basePath, ".config.json"));
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<BasePathConfig>(text);
                AppSettingsProvider.ReadOnlyFolders = model.Readonly;
                // "structure": "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss.ext/"
                AppSettingsProvider.Structure = model.Structure;
            }

            AppSettingsProvider.BasePath = basePath;
            Enum.TryParse<AppSettingsProvider.DatabaseTypeList>(databaseType, out var databaseTypeEnum);
            AppSettingsProvider.DatabaseType = databaseTypeEnum;
            AppSettingsProvider.DbConnectionString = defaultConnection; // First database type
            AppSettingsProvider.ThumbnailTempFolder = thumbnailTempFolder;
            AppSettingsProvider.ExifToolPath = exifToolPath;

            if(AppSettingsProvider.Verbose) Console.WriteLine("DatabaseType: " + AppSettingsProvider.DatabaseType.ToString() );

        }

        private static string ReadTextFromObjOrEnv(string name, JObject obj = null)
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
            if (string.IsNullOrWhiteSpace(thumbnailTempFolder)) return thumbnailTempFolder;
            
            if (thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1,
                1) != Path.DirectorySeparatorChar.ToString())
            {
                thumbnailTempFolder += Path.DirectorySeparatorChar.ToString();
            }
            return thumbnailTempFolder;
        }
        
        public static string PrefixDbSlash(string thumbnailTempFolder) { 
            // Add normal linux slash to beginning of the configuration
            if (thumbnailTempFolder.Length == 0) return "/";
            
            if (thumbnailTempFolder.Substring(0,1) != "/")
            {
                thumbnailTempFolder = "/" + thumbnailTempFolder;
            }
            return thumbnailTempFolder;
        }
        
//        public static string PrefixBackslash(string thumbnailTempFolder) { 
//            // Add BackSlash to beginning of the configuration
//            if (thumbnailTempFolder.Length == 0) return "/";
//            
//            if (thumbnailTempFolder.Substring(0,1) != Path.DirectorySeparatorChar.ToString())
//            {
//                thumbnailTempFolder = Path.DirectorySeparatorChar.ToString() + thumbnailTempFolder;
//            }
//            return thumbnailTempFolder;
//        }

        
    }
}
