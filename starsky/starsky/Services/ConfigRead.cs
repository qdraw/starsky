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

        public static string AddSlash(string thumbnailTempFolder) { 
            // Add backSlash to configuration // or \\
            // Platform depended feature
            if (string.IsNullOrWhiteSpace(thumbnailTempFolder)) return thumbnailTempFolder;
            
            if (thumbnailTempFolder.Substring(thumbnailTempFolder.Length - 1,
                    1) != Path.DirectorySeparatorChar.ToString())
            {
                thumbnailTempFolder += "/";
            }
            return thumbnailTempFolder;
        }
        

        public static string PrefixDbSlash(string thumbnailTempFolder) { 
            // Add normal linux slash to beginning of the configuration
            if (string.IsNullOrWhiteSpace(thumbnailTempFolder)) return "/";
            
            if (thumbnailTempFolder.Substring(0,1) != "/")
            {
                thumbnailTempFolder = "/" + thumbnailTempFolder;
            }
            return thumbnailTempFolder;
        }
        
        public static string RemovePrefixDbSlash(string subpath) { 
            // Remove linux slash to beginning of the configuration
            if (string.IsNullOrWhiteSpace(subpath)) return "/";
            
            if (subpath.Substring(0,1) == "/")
            {
                subpath = subpath.Remove(0, 1);
            }
            return subpath;
        }
      
        
    }
}
