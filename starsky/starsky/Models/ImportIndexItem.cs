using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
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
        
        
        // Depends on App Settings for storing values
        // Depends on BasePathConfig for setting default values
        // Imput required:
        // SourceFullFilePath= createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime
        public string ParseFileName()
        {
            if (string.IsNullOrWhiteSpace(SourceFullFilePath)) return string.Empty;
            var fileExtenstion = Files.GetImageFormat(SourceFullFilePath).ToString();

            var structuredFileName = AppSettingsProvider.Structure.Split("/").LastOrDefault();
            if (structuredFileName == null) return null;

            // Replace Astriks
            structuredFileName = structuredFileName?.Replace("*", "");

            var extPosition = structuredFileName.IndexOf(".ext", StringComparison.Ordinal);
            structuredFileName = structuredFileName.Substring(0, extPosition);

            var fileName = DateTime.ToString(structuredFileName, CultureInfo.InvariantCulture);
            fileName += "." + fileExtenstion;
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
                    return "/";
                }
                return ConfigRead.PrefixBackslash(_subFolder);
            }
            set { _subFolder += value + "/" ; }
        }

        private string SelectFirstDirectory(string parentItem, string parsedItem)
        {
            var childDirectories = Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem), parsedItem).ToList();
            childDirectories = childDirectories.Where(p => p[0].ToString() != ".").OrderBy(s => s).ToList();
            var childDirectory = childDirectories.FirstOrDefault();
            return childDirectory;
        }
        
        // Depends on App Settings /BasePathConfig
        public List<string> ParseSubfolders()
        {
            var patternList = AppSettingsProvider.Structure.Split("/").ToList();
            var parsedList = ParseListDateFormat(patternList, DateTime);
            if (parsedList.Count >= 1)
            {
                parsedList.RemoveAt(parsedList.Count - 1);
            }

            foreach (var parsedItem in parsedList)
            {
                var parentItem = SubFolder;
                string childDirectory = null;
                foreach (var test in Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem)))
                {
                    Console.WriteLine(test);
                }
                    
                if (Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem)).Length != 0)
                {
                    childDirectory = SelectFirstDirectory(parentItem, parsedItem);
                }

//                if (childDirectory == null)
//                {
//                    childDirectory = SelectFirstDirectory(parentItem, parsedItem.Replace("*",string.Empty));
//                }

                if (childDirectory != null)
                {
                    SubFolder = FileIndexItem.FullPathToDatabaseStyle(childDirectory);
                }
                
                
                
//                var parentItem = SubFolder;
//                SubFolder = parsedItem;
//
//                if (FileIndexItem.DatabasePathToFilePath(SubFolder) == null)
//                {
//                    if (parsedItem.Contains("*"))
//                    {
//                        var childDirectories = Directory.GetDirectories(FileIndexItem.DatabasePathToFilePath(parentItem), parsedItem).ToList();
//                        childDirectories = childDirectories.Where(p => p[0].ToString() != ".").OrderBy(s => s).ToList();
//
//                    }
//                    Console.WriteLine(parentItem);
//                }
                
            }

            Console.WriteLine(SubFolder);
            
            return parsedList;
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