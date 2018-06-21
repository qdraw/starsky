using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using starsky.Helpers;
using starsky.Services;

namespace starsky.Models
{
    public class ImportIndexItem
    {
        public int Id { get; set; }
        public string FileHash { get; set; }
        public DateTime AddToDatabase { get; set; }

        public string SourceFullFilePath { get; set; }
        public DateTime DateTime{ get; set; }
        
        // Caching to have it after you use the afterDelete flag
        private string FileName { get; set; }

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

            // Replace Astriks
            structuredFileName = structuredFileName?.Replace("*", "");

            if (structuredFileName.Contains(".ext"))
            {
                var extPosition = structuredFileName.IndexOf(".ext", StringComparison.Ordinal);
                structuredFileName = structuredFileName.Substring(0, extPosition);
            }


            var fileName = DateTime.ToString(structuredFileName, CultureInfo.InvariantCulture);
            fileName += "." + fileExtenstion;
            
            // Caching to have it after you use the afterDelete flag
            FileName = fileName;
            return fileName;
        }

        private string  _subFolder  { get; set; }

        [NotMapped]
        public string SubFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_subFolder))
                {
                    return string.Empty;
                }
                return ConfigRead.PrefixBackslash(_subFolder);
            }
            set { _subFolder = value + "/" ; } // +?
        }

        private string SelectFirstDirectory(string parentItem, string parsedItem)
        {
            var childDirectories = Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem), parsedItem).ToList();
            childDirectories = childDirectories.Where(p => p[0].ToString() != ".").OrderBy(s => s).ToList();
            var childDirectory = childDirectories.FirstOrDefault();
            return childDirectory;
        }
        
        // Depends on App Settings /BasePathConfig
        public string ParseSubfolders(bool createFolder = true)
        {
            // If command running twiche you will get /tr/tr (when tr is your single folder name)
            _subFolder = string.Empty;
            
            var patternList = AppSettingsProvider.Structure.Split("/").ToList();
            var parsedList = ParseListDateFormat(patternList, DateTime);
            if (parsedList.Count >= 1)
            {
                parsedList.RemoveAt(parsedList.Count - 1);
            }

            foreach (var parsedItem in parsedList)
            {
                var parentItem = SubFolder;
                string childFullDirectory = null;

                if (Directory.Exists(FileIndexItem.DatabasePathToFilePath(parentItem)) &&
                    Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem)).Length != 0)
                {
                    childFullDirectory = SelectFirstDirectory(parentItem, parsedItem);
                }

                if (childFullDirectory == null)
                {
                    childFullDirectory = FileIndexItem.DatabasePathToFilePath(
                                          SubFolder
                                          + parsedItem.Replace("*", string.Empty),
                                        false // folder does not exist
                                        );
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

        private List<string> ParseListDateFormat(List<string> patternList, DateTime fileDateTime)
        {
            var parseListDate = new List<string>();

            patternList = PatternListInput(patternList, "*", "_!x_");

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
            
            return parseListDate;
        }
        
    }
}