using System;
using System.Collections.Generic;
using System.Globalization;
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
        public string ParseFileName()
        {
            if (string.IsNullOrWhiteSpace(SourceFullFilePath)) return string.Empty;
            var fileExtenstion = Files.GetImageFormat(SourceFullFilePath).ToString();

            // Replace Astriks
            var structuredFileName = AppSettingsProvider.Structure.Split("/").LastOrDefault();
            structuredFileName = structuredFileName?.Replace("*", "");

            // Position of dot ext
            var searchForExt = ".ext".ToCharArray();
            var extPosition = AllIndexesOf(structuredFileName, searchForExt.ToString()).FirstOrDefault();
//            char[] ch = somestring.ToCharArray();
//            ch[3] = 'X'; // index starts at 0!
            
            var fileName = DateTime.ToString(structuredFileName, CultureInfo.InvariantCulture).ToCharArray();
            for (var i = extPosition; i < searchForExt.Length; i++)
            {
                
            }

//            ch[3] = 'X'; // index starts at 0!

            
            
            Console.WriteLine();
//            var structureList = AppSettingsProvider.Structure;
//
//            var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
//
//            
//            fileNameStructureList = _parseListDateFormat(fileNameStructureList, DateTime);
//                
//            foreach (var item in fileNameStructureList)
//            {
//                Regex rgx = new Regex(".ex[A-Z]$");
//                var result = rgx.Replace(item, "." + fileExtenstion);
//                return result;
//            }
            return string.Empty;
        }
        
//        private static List<string> _parseListDateFormat(List<string> patternList, DateTime fileDateTime)
//        {
//            var parseListDate = new List<string>();
//            foreach (var patternItem in patternList)
//            {
//                if (patternItem == "/") return patternList;
//                var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
//                parseListDate.Add(item);
//            }
//            return parseListDate;
//        }
//        
//        AllIndexesOf(string str, string value)
        
        public List<int> AllIndexesOf(string inputString, string searchFor) {
            if (String.IsNullOrEmpty(searchFor))
                throw new ArgumentException("the string to find may not be empty", nameof(searchFor));
            List<int> indexes = new List<int>();
            for (int index = 0;; index += searchFor.Length) {
                index = inputString.IndexOf(searchFor, index, StringComparison.Ordinal);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
    }
}