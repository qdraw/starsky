using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using starsky.Models;

namespace starsky.Services
{
    public static class ExifRead
    {
        public static FileIndexItem ReadExifFromFile(string fileFullPath)
        {
            var item = new FileIndexItem();
            
            try
            {
                var allExifItems = ImageMetadataReader.ReadMetadata(fileFullPath);
                var colorClassSting = string.Empty;
    
                foreach (var exifItem in allExifItems)
                {
                    
                    //Console.WriteLine(exifItem);
                    //foreach (var tag in exifItem.Tags) Console.WriteLine($"[{exifItem.Name}] {tag.Name} = {tag.Description}");
    
    
                    var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Keywords");
                    if (tCounts >= 1)
                    {
                        item.Tags = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "IPTC" && p.Name == "Keywords")?.Description.ToLower();
                        if (!string.IsNullOrWhiteSpace(item.Tags))
                        {
                            item.Tags = item.Tags.Replace(";", ", ");
                        }
                    }
    
                    // Colour Class => ratings
                    var ratingCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"));
                    if (ratingCounts >= 1)
                    {
                        var prefsTag = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "IPTC" && p.Name.Contains("0x02dd"))?.Description;
    
                        //     0:1:0:-00001
                        //     ~~~~~~
                        //     0:8:0:-00001
                        
                        if (!string.IsNullOrWhiteSpace(prefsTag))
                        {
                            var prefsTagSplit = prefsTag.Split(":");
                            colorClassSting = prefsTagSplit[1];     
                        }
                    }
    
                    var dtCounts = exifItem.Tags.Count(p => p.DirectoryName == "Exif SubIFD" && p.Name == "Date/Time Digitized");
                    if (dtCounts >= 1)
                    {
    
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
                item.SetColorClass(colorClassSting);
                
            }
            catch (ImageProcessingException e)
            {
                item.Tags = "ImageProcessingException";
                Console.WriteLine(e);
            }
            
            return item;
        }

        //public static string WriteExifFromFile(string fileFullPath)
        //{
        //    fileFullPath =
        //        @"2E5NEHNAF2XSQCK4ZJCYMJGGBQ.jpg";

        //    fileFullPath = "20180101_000337.jpg";

        //    //var options = new ExifToolOptions(){ExifToolPath = "exiftool(-k).exe" };

        //    //var et = new ExifTool(options);
        //    //var tags = et.GetTags(fileFullPath);


        //    //foreach (var tag in tags)
        //    //{
        //    //    Console.WriteLine(tag);
        //    //}


        //    //Console.WriteLine(tags);


        //    //var adapter = new JpegMetadataAdapter(fileFullPath);
        //    //Console.WriteLine(adapter.Metadata.Keywords);


        //    ////using (var outputStream = new FileStream(thumbPath, FileMode.CreateNew))
        //    //using (var inputStream = File.OpenRead(fileFullPath))
        //    //{
        //    //    var file = ImageFile.FromStream(inputStream);
        //    //    foreach (ExifProperty item in file.Properties)
        //    //    {
        //    //        Console.WriteLine(item);
        //    //    }
        //    //}

        //    try
        //    {
        //        // FileIndexItem.DatabasePathToFilePath(item.FilePath))
        //        ImageFile file = ImageFile.FromFile(fileFullPath);
        //        // Read metadata
        //        foreach (ExifProperty item in file.Properties)
        //        {
        //            Console.WriteLine(item.Name + "~  " + item.Value);
        //            // Do something with meta data
        //        }

        //    }
        //    catch (System.ArgumentException e)
        //    {
        //        Console.WriteLine(e);
        //    }
        
        //    return null;

        //}
    }
}
