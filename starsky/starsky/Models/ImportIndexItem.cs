using System;
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
        private readonly AppSettings _appSettings;

        //  In order to create an instance of 'ImportIndexItem'
        // EF requires that a parameterless constructor be declared.
        public ImportIndexItem()
        {
        }
        
        public ImportIndexItem(AppSettings appSettings)
        {
            _appSettings = appSettings;
            Structure = _appSettings.Structure;
        }

        
        public int Id { get; set; }
        public string FileHash { get; set; }

        public DateTime AddToDatabase { get; set; }

        public DateTime DateTime{ get; set; } // Time of the photo
        
        // Caching to have it after you use the afterDelete flag
        private string FileName { get; set; }

        [NotMapped]
        public string SourceFullFilePath { get; set; }

        // Defaults to _appSettings.Structure
        // Feature to overwrite system structure by request
        [NotMapped] 
        public string Structure { get; set; }

        
        // Depends on App Settings for storing values
        // Depends on BasePathConfig for setting default values
        // Imput required:
        // SourceFullFilePath= createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime
        public string ParseFileName(bool checkIfExist = true)
        {
            if (string.IsNullOrWhiteSpace(SourceFullFilePath)) return string.Empty;
            var imageFormatExtenstion = Files.GetImageFormat(SourceFullFilePath);
            
            var fileExtension = Path.GetExtension(SourceFullFilePath).Replace(".",string.Empty).ToLower();

            if (imageFormatExtenstion == Files.ImageFormat.notfound)
            {
                // Caching feature to have te Path and url after you deleted the orginal in the ImportIndexItem context
                if (FileName != null) return FileName;
                if(checkIfExist) throw new FileNotFoundException("source image not found");
            }
            
            var structuredFileName = Structure.Split("/").LastOrDefault();
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
            fileName += "." + fileExtension;
            
            // Escape feature to Restore (filenamebase)
            if (fileName.Contains("_!q_")) // filenamebase
            {
                fileName = fileName.Replace("_!q_", Path.GetFileNameWithoutExtension(SourceFullFilePath));
            }
            
            // Caching to have it after you use the afterDelete flag
            FileName = fileName;
            return fileName;
        }

        public DateTime ParseDateTimeFromFileName()
        {
            // Depends on 'AppSettingsProvider.Structure'
            // depends on SourceFullFilePath
            if(string.IsNullOrEmpty(SourceFullFilePath)) {return new DateTime();}

            var fileName = Path.GetFileNameWithoutExtension(SourceFullFilePath);
            
            // Replace magic string from import
            fileName = fileName.Replace("_import_", string.Empty);
            
            // Replace Astriks > escape all options
            var structuredFileName = Structure.Split("/").LastOrDefault();
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
                return dateTime;
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
            
            if (dateTime.Year >= 2)
            {
                DateTime = dateTime;
                return dateTime;
            }

            // when using /yyyymmhhss_{filenamebase}.jpg
            if(fileName.Length >= structuredFileName.Length) {
                
                fileName = fileName.Substring(0, structuredFileName.Length);
                
                DateTime.TryParseExact(fileName, 
                    structuredFileName, 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, 
                    out dateTime);
            }
            
            DateTime = dateTime;
            return dateTime;
        }


        [NotMapped]
        public string SubFolder  { get; set; }

        public List<string> SearchSubDirInDirectory(string parentItem, string parsedItem)
        {
            if (_appSettings == null) throw new FieldAccessException("use with _appsettings");
            var childDirectories = Directory.GetDirectories(_appSettings.DatabasePathToFilePath(parentItem), parsedItem).ToList();
            childDirectories = childDirectories.Where(p => p[0].ToString() != ".").OrderBy(s => s).ToList();
            return childDirectories;
        }

        private IEnumerable<string> ListParser()
        {
            var patternList = Structure.Split("/").ToList();
            var parsedList = ParseListBasePathAndDateFormat(patternList, DateTime);

            if (parsedList.Count == 1)
            {
                return new List<string>{string.Empty};
            }
                
            if (parsedList.Count >= 2)
            {
                parsedList.RemoveAt(parsedList.Count - 1);
            }

            // database slash to first item
            parsedList[0] = "/" + parsedList[0];
            return parsedList;
        }
       
        // Depends on App Settings /BasePathConfig
        public string ParseSubfolders(bool createFolder = true)
        {
            if (_appSettings == null) throw new FieldAccessException("use with _appsettings");

            // If command running twiche you will get /tr/tr (when tr is your single folder name)
            SubFolder = string.Empty;

            var parsedList = ListParser();
            
            foreach (var parsedItem in parsedList)
            {
                var parentItem = SubFolder;
                string childFullDirectory = null;

                if (Directory.Exists(_appSettings.DatabasePathToFilePath(parentItem)) &&
                    Directory.GetDirectories(_appSettings.DatabasePathToFilePath(parentItem)).Length != 0)
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
                    childFullDirectory = _appSettings.DatabasePathToFilePath(childDirectory,false);

                    if (createFolder)
                    {
                        Console.WriteLine("childFullDirectory");
                        Console.WriteLine(childFullDirectory);
                        Directory.CreateDirectory(childFullDirectory);
                    }
                }
                SubFolder = _appSettings.FullPathToDatabaseStyle(childFullDirectory);
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