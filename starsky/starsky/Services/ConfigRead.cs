using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using starsky.Models;

namespace starsky.Services
{
    public class ConfigRead
    {
        public static void SetAppSettingsProvider()
        {
            string basePath;
            string defaultConnection;
            string databaseType;
            string thumbnailTempFolder;
            string exifToolPath;

            try
            {
                Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json");
                string text = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json");

                JObject obj = JObject.Parse(text);

                basePath = (string)obj["ConnectionStrings"]["STARSKY_BASEPATH"];

                if (basePath == null) throw new FileNotFoundException("basePath==null");

                // remove latest backslash
                if (basePath.Substring(basePath.Length - 1, 1) == Path.DirectorySeparatorChar.ToString())
                {
                    basePath = basePath.Substring(0, basePath.Length - 1);
                }

                defaultConnection = (string)obj["ConnectionStrings"]["DefaultConnection"];
                databaseType = (string)obj["ConnectionStrings"]["DatabaseType"];
                thumbnailTempFolder = (string)obj["ConnectionStrings"]["ThumbnailTempFolder"];

                if (thumbnailTempFolder == null) throw new FileNotFoundException("thumbnailTempFolder==null");

                // Add backSlash to configuration
                if (thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1, 1) != Path.DirectorySeparatorChar.ToString())
                {
                    thumbnailTempFolder += Path.DirectorySeparatorChar.ToString();
                }


                exifToolPath = (string)obj["ConnectionStrings"]["ExifToolPath"];

                if (exifToolPath == null) throw new FileNotFoundException("ExifToolPath==null");

                // remove latest backslash
                if (exifToolPath.Substring(exifToolPath.Length - 1, 1) == Path.DirectorySeparatorChar.ToString())
                {
                    exifToolPath = exifToolPath.Substring(0, exifToolPath.Length - 1);
                }

                //if (!File.Exists(exifToolPath))
                //{
                //    throw new FileLoadException("ExifToolPath not found");
                //}

            }
            catch (FileNotFoundException)
            {
                basePath = null;
                defaultConnection = null;
                databaseType = null;
                thumbnailTempFolder = null;
                exifToolPath = null;
            }

            if (basePath != null)
            {
                AppSettingsProvider.BasePath = basePath;
                AppSettingsProvider.DbConnectionString = defaultConnection;
                AppSettingsProvider.DatabaseType = databaseType == "mysql" ? AppSettingsProvider.DatabaseTypeList.Mysql : AppSettingsProvider.DatabaseTypeList.Sqlite;
                AppSettingsProvider.ThumbnailTempFolder = thumbnailTempFolder;
                AppSettingsProvider.ExifToolPath = exifToolPath;

                //AppSettingsProvider.DbConnectionString = "Data Source=../starsky/data.db";

                //new SettingsCli().BasePath = basePath;
                //new SettingsCli().DefaultConnection = defaultConnection;
                //AppSettingsProvider.BasePath = "Z:\\data\\isight\\2018";
                //AppSettingsProvider.DbConnectionString = "Data Source=../starsky/data.db";
            }
            else
            {
                throw new System.ArgumentException("BasePath or ConnectionStrings or ThumbnailTempFolder not readed");
            }

        }
    }
}
