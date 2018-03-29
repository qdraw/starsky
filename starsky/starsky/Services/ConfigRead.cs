﻿using System;
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
            string basePath;
            string defaultConnection;
            string databaseType;
            string thumbnailTempFolder;
            string exifToolPath;

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json"))
            {
                string text =
                    File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "appsettings.json");
                JObject obj = JObject.Parse(text);

                // >>> Base Path of Orginal images <<<
                basePath = (string)obj["ConnectionStrings"]["STARSKY_BASEPATH"];
                IsSettingEmpty(basePath, "STARSKY_BASEPATH");
                basePath = RemoveLatestBackslash(basePath);

                // >>> Default Connection SQL <<<
                defaultConnection = (string)obj["ConnectionStrings"]["DefaultConnection"];
                IsSettingEmpty(defaultConnection, "defaultConnection");

                // >>> Database Type <<<
                databaseType = (string)obj["ConnectionStrings"]["DatabaseType"];
                IsSettingEmpty(databaseType, "DatabaseType");

                // >>> Thumbnail temp folder <<<
                thumbnailTempFolder = (string)obj["ConnectionStrings"]["ThumbnailTempFolder"];
                IsSettingEmpty(thumbnailTempFolder, "ThumbnailTempFolder");
                thumbnailTempFolder = AddBackslash(thumbnailTempFolder);

                // >>> ExifTool path => /usr/bin/exiftool <<<
                exifToolPath = (string)obj["ConnectionStrings"]["ExifToolPath"];
                IsSettingEmpty(exifToolPath, "ExifToolPath");
                exifToolPath = RemoveLatestBackslash(exifToolPath);
            }
            else
            {
                // >>> Base Path of Orginal images <<<
                basePath = Environment.GetEnvironmentVariable("STARSKY_BASEPATH");
                IsSettingEmpty(basePath, "STARSKY_BASEPATH");
                basePath = RemoveLatestBackslash(basePath);

                // >>> Default Connection SQL <<<
                defaultConnection = Environment.GetEnvironmentVariable("DefaultConnection");
                IsSettingEmpty(defaultConnection, "defaultConnection");

                // >>> Database Type <<<
                databaseType = Environment.GetEnvironmentVariable("DatabaseType");
                IsSettingEmpty(databaseType, "DatabaseType");

                // >>> Thumbnail temp folder <<<
                thumbnailTempFolder = Environment.GetEnvironmentVariable("ThumbnailTempFolder");
                IsSettingEmpty(thumbnailTempFolder, "ThumbnailTempFolder");
                thumbnailTempFolder = AddBackslash(thumbnailTempFolder);

                // >>> ExifTool path => /usr/bin/exiftool <<<
                exifToolPath = Environment.GetEnvironmentVariable("ExifToolPath");
                IsSettingEmpty(exifToolPath, "ExifToolPath");
                exifToolPath = RemoveLatestBackslash(exifToolPath);
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

        public static bool IsSettingEmpty(string setting, string name = "")
        {
            if (string.IsNullOrWhiteSpace(setting)) throw new FileNotFoundException(name + " ==null");
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
