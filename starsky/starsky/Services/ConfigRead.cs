using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
            var addMemoryCache = ReadTextFromObjOrEnv("AddMemoryCache", obj, false); // false means is optional
            var structure = ReadTextFromObjOrEnv("Structure", obj, false); // false means is optional
            var readOnlyFolders = ReadTextFromObjOrEnvListOfItems("ReadOnlyFolders", obj, false); // false means is optional

            SetAppSettingsProvider(basePath,defaultConnection,databaseType,thumbnailTempFolder,
                exifToolPath,addMemoryCache, structure, readOnlyFolders);
        }

        public static void SetAppSettingsProvider(
            string basePath,string defaultConnection,string databaseType,string thumbnailTempFolder, 
            string exifToolPath, string addMemoryCache, string structure, List<string> readonlyFolders)
        {

            thumbnailTempFolder = AddBackslash(thumbnailTempFolder);
            basePath = AddBackslash(basePath);

            AppSettingsProvider.BasePath = basePath;
            Enum.TryParse<AppSettingsProvider.DatabaseTypeList>(databaseType, out var databaseTypeEnum);
            AppSettingsProvider.DatabaseType = databaseTypeEnum;
            AppSettingsProvider.DbConnectionString = defaultConnection; // First database type
            AppSettingsProvider.ThumbnailTempFolder = thumbnailTempFolder;
            AppSettingsProvider.ExifToolPath = exifToolPath;
            // When using in combination with /api/env > please update it also in EnvViewModel()
                        
            bool.TryParse(addMemoryCache, out var memoryCache);
            if (string.IsNullOrWhiteSpace(addMemoryCache)) memoryCache = true;
            AppSettingsProvider.AddMemoryCache = memoryCache;
            
            AppSettingsProvider.Structure = structure;
            
//            if (string.IsNullOrWhiteSpace(readonlyFolders)) readonlyFolders = "[]";
//            JArray.Parse(readonlyFolders).ToObject<List<string>>();
            AppSettingsProvider.ReadOnlyFolders = readonlyFolders;

            if(AppSettingsProvider.Verbose) Console.WriteLine("DatabaseType: " + AppSettingsProvider.DatabaseType.ToString() );
        }
        
        public static List<string> ReadTextFromObjOrEnvListOfItems(string name, JObject obj = null, bool throwError = true)
        {
            // input=text, nameofvar=text 
            // >>> Base Path of Orginal images <<<
            var value = Environment.GetEnvironmentVariable(name);

            var listOfStrings = new List<string>();
            // >>> Base Path of Orginal images <<<
            if(obj != null && IsSettingEmpty(value, name)) {
                JArray array = (JArray) obj["ConnectionStrings"][name];
                try
                {
                    listOfStrings = array.ToObject<List<string>>();
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e);
                    if(throwError) throw;
                }
                value = listOfStrings.ToString();
            }

            IsSettingEmpty(value, name, throwError);
            if (value != null)
            {
                listOfStrings = JArray.Parse(value).ToObject<List<string>>();
            }
            
            return listOfStrings;
        }

        private static string ReadTextFromObjOrEnv(string name, JObject obj = null, bool throwError = true)
        {
            // input=text, nameofvar=text 
            // >>> Base Path of Orginal images <<<
            var value = Environment.GetEnvironmentVariable(name);

            // >>> Base Path of Orginal images <<<
            if(obj != null && IsSettingEmpty(value, name)) {
                value = (string)obj["ConnectionStrings"][name];
                IsSettingEmpty(value, name, throwError);
                value = RemoveLatestBackslash(value);
            }
            IsSettingEmpty(value, name, throwError);
            value = RemoveLatestBackslash(value);
            return value;
        }

        public static bool IsSettingEmpty(string setting, string name = "", bool throwError = false)
        {
            if (string.IsNullOrWhiteSpace(setting) && throwError) throw new FileNotFoundException(name + " ==null");
            if (string.IsNullOrWhiteSpace(setting)) return true;
            return false;
        }

        public static string RemoveLatestBackslash(string basePath = "/")
        {
            if (string.IsNullOrWhiteSpace(basePath)) return null;

            // Depends on Platform
            if (basePath == "/") return basePath;
            
            // remove latest backslash
            if (basePath.Substring(basePath.Length - 1, 1) == Path.DirectorySeparatorChar.ToString())
            {
                basePath = basePath.Substring(0, basePath.Length - 1);
            }
            return basePath;
        }


        public static string RemoveLatestSlash(string basePath)
        {
            // on all platforms the same
            if (string.IsNullOrWhiteSpace(basePath) || basePath == "/" ) return string.Empty;

            // remove latest slash
            if (basePath.Substring(basePath.Length - 1, 1) == "/")
            {
                basePath = basePath.Substring(0, basePath.Length - 1);
            }
            return basePath;
        }

        public static string AddBackslash(string thumbnailTempFolder) { 
            // Add backSlash to configuration // or \\
            // Platform depended feature
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
      
        
    }
}
