﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using starsky.Helpers;
using starsky.Services;

namespace starsky.Models
{
    public class ImportIndexItem
    {
        public int Id { get; set; }
        public string FileHash { get; set; }

        public DateTime AddToDatabase { get; set; }

        public DateTime DateTime{ get; set; } // Time of the photo
        
        // Caching to have it after you use the afterDelete flag
        private string FileName { get; set; }

        [NotMapped]
        public string SourceFullFilePath { get; set; }
        
        // Depends on App Settings for storing values
        // Depends on BasePathConfig for setting default values
        // Imput required:
        // SourceFullFilePath= createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime
        public string ParseFileName()
        {
            if (string.IsNullOrWhiteSpace(SourceFullFilePath)) return string.Empty;
            var fileExtenstion = Files.GetImageFormat(SourceFullFilePath);

            if (fileExtenstion == Files.ImageFormat.notfound)
            {
                // Caching feature to have te Path and url after you deleted the orginal in the ImportIndexItem context
                if (FileName != null) return FileName;
                throw new FileNotFoundException("source image not found");
            }
            
            var structuredFileName = AppSettingsProvider.Structure.Split("/").LastOrDefault();
            if (structuredFileName == null) return null;

            // Escape feature to Replace Astriks
            structuredFileName = structuredFileName?.Replace("*", "");

            if (structuredFileName.Contains(".ext"))
            {
                var extPosition = structuredFileName.IndexOf(".ext", StringComparison.Ordinal);
                structuredFileName = structuredFileName.Substring(0, extPosition);
            }

            // Escape feature to {filenamebase} replace
            structuredFileName = structuredFileName.Replace("{filenamebase}", "_!q_");

            // Parse the DateTime to a string
            var fileName = DateTime.ToString(structuredFileName, CultureInfo.InvariantCulture);
            fileName += "." + fileExtenstion;
            
            // Escape feature to Restore {filenamebase}
            if (fileName.Contains("_!q_")) // filenamebase
            {
                fileName = fileName.Replace("_!q_", Path.GetFileNameWithoutExtension(SourceFullFilePath));
            }
            
            // Caching to have it after you use the afterDelete flag
            FileName = fileName;
            return fileName;
        }

        public void ParseDateTimeFromFileName()
        {
            // Depends on 'AppSettingsProvider.Structure'
            // depends on SourceFullFilePath
            if(string.IsNullOrEmpty(SourceFullFilePath)) {return;}

            var fileName = Path.GetFileNameWithoutExtension(SourceFullFilePath);
            
            // Replace Astriks > escape all options
            var structuredFileName = AppSettingsProvider.Structure.Split("/").LastOrDefault();
            structuredFileName = structuredFileName.Replace("*", "");
            structuredFileName = structuredFileName.Replace(".ext", string.Empty);
            structuredFileName = structuredFileName.Replace("{filenamebase}", string.Empty);
            
            DateTime.TryParseExact(fileName, 
                structuredFileName, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var dateTime);

            if (dateTime.Year >= 2)
            {
                DateTime = dateTime;
                return;
            }
            
            // Now retry it and replace special charaters from string
            Regex pattern = new Regex("-|_| |;|\\.|:");
            fileName = pattern.Replace(fileName,string.Empty);
            structuredFileName = pattern.Replace(structuredFileName,string.Empty);
                
            DateTime.TryParseExact(fileName, 
                structuredFileName, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out dateTime);
            
            DateTime = dateTime;
        }


        [NotMapped]
        public string SubFolder  { get; set; }

        public List<string> SearchSubDirInDirectory(string parentItem, string parsedItem)
        {
            var childDirectories = Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem), parsedItem).ToList();
            childDirectories = childDirectories.Where(p => p[0].ToString() != ".").OrderBy(s => s).ToList();
            return childDirectories;
        }
        
       
        // Depends on App Settings /BasePathConfig
        public string ParseSubfolders(bool createFolder = true)
        {
            // If command running twiche you will get /tr/tr (when tr is your single folder name)
            SubFolder = string.Empty;
            
            var patternList = AppSettingsProvider.Structure.Split("/").ToList();
            var parsedList = ParseListBasePathAndDateFormat(patternList, DateTime);

            if (parsedList.Count == 1)
            {
                return string.Empty;
            }
                
            if (parsedList.Count >= 2)
            {
                parsedList.RemoveAt(parsedList.Count - 1);
            }

            // database slash to first item
            parsedList[0] = "/" + parsedList[0];
            
            foreach (var parsedItem in parsedList)
            {
                var parentItem = SubFolder;
                string childFullDirectory = null;

                if (Directory.Exists(FileIndexItem.DatabasePathToFilePath(parentItem)) &&
                    Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem)).Length != 0)
                {
                    // add backslash
                    var noSlashInParsedItem = parsedItem.Replace("/", string.Empty);
                    
                    childFullDirectory = ConfigRead.AddBackslash(SearchSubDirInDirectory(parentItem, noSlashInParsedItem).FirstOrDefault());
                    /// only first item
                    if (SubFolder == string.Empty && childFullDirectory != null)
                    {
                        childFullDirectory = Path.DirectorySeparatorChar + childFullDirectory;
                    }
                }

                if (childFullDirectory == null)
                {
                    var childDirectory = SubFolder + parsedItem.Replace("*", string.Empty) + "/";
                    childFullDirectory = FileIndexItem.DatabasePathToFilePath(childDirectory,false);

                    if (createFolder)
                    {
                        Console.WriteLine("childFullDirectory");
                        Console.WriteLine(childFullDirectory);
                        Directory.CreateDirectory(childFullDirectory);
                    }
                }
                SubFolder = FileIndexItem.FullPathToDatabaseStyle(childFullDirectory);
            }

            return SubFolder;
        }

        // Escape feature
        private List<string> PatternListInput(List<string> patternList, string search, string replace)
        {
            var patternListReturn = new List<string>();
            foreach (var t in patternList)
            {
                patternListReturn.Add(t.Replace(search, replace));
            }
            return patternListReturn;
        }

        private List<string> ParseListBasePathAndDateFormat(List<string> patternList, DateTime fileDateTime)
        {
            var parseListDate = new List<string>();

            patternList = PatternListInput(patternList, "*", "_!x_");
            patternList = PatternListInput(patternList, "{filenamebase}", "_!q_");

            foreach (var patternItem in patternList)
            {
                if (patternItem == "/" ) return patternList;

                if(!string.IsNullOrWhiteSpace(patternItem))
                {
                    var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
                    parseListDate.Add(item);
                }
            }

            parseListDate = PatternListInput(parseListDate, "_!x_", "*");
            parseListDate = PatternListInput(parseListDate, "_!q_", Path.GetFileNameWithoutExtension(SourceFullFilePath));

            return parseListDate;
        }
        
    }
}