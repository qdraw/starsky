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

            var extPosition = structuredFileName.IndexOf(".ext", StringComparison.Ordinal);
            structuredFileName = structuredFileName.Substring(0, extPosition);
            
            var fileName = DateTime.ToString(structuredFileName, CultureInfo.InvariantCulture);
            fileName += "." + fileExtenstion;
            return fileName;
        }
        
        private static List<string> _parseListDateFormat(List<string> patternList, DateTime fileDateTime)
        {
            var parseListDate = new List<string>();
            foreach (var patternItem in patternList)
            {
                if (patternItem == "/") return patternList;
                var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
                parseListDate.Add(item);
            }
            return parseListDate;
        }
        
    }
}