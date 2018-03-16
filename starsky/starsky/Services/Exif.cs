using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using starsky.Models;

namespace starsky.Services
{
    public class Exif
    {
        public static FileIndexItem ReadExifFromFile(string fileFullPath)
        {
            var item = new FileIndexItem();
            var allExifItems = ImageMetadataReader.ReadMetadata(fileFullPath);

            foreach (var exifItem in allExifItems)
            {
                
                //Console.WriteLine(exifItem);

                var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Keywords");
                if (tCounts >= 1)
                {
                    item.Tags = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "IPTC" && p.Name == "Keywords")?.Description.ToLower();
                }

                var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized");
                if (dtCounts >= 1)
                {
                    //foreach (var tag in exifItem.Tags) Console.WriteLine($"[{exifItem.Name}] {tag.Name} = {tag.Description}");

                    var dateString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized")?.Description;

                    // https://odedcoster.com/blog/2011/12/13/date-and-time-format-strings-in-net-understanding-format-strings/
                    //2018:01:01 11:29:36
                    string pattern = "yyyy:MM:dd HH:mm:ss";
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime.TryParseExact(dateString, pattern, provider, DateTimeStyles.AdjustToUniversal, out var itemDateTime);

                    if (itemDateTime.Year == 1 && itemDateTime.Month == 1)
                    {
                        dateString = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Original")?.Description;
                        DateTime.TryParseExact(dateString, pattern, provider, DateTimeStyles.AdjustToUniversal, out itemDateTime);
                    }

                    item.DateTime = itemDateTime;
                }

            }

            return item;
        }

    }
}
