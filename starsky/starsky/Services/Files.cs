using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using starsky.Models;
using MetadataExtractor;

namespace starsky.Services
{
    public class Files
    {
        //public string CheckMd5(string filename)
        //{
        //    using (var md5 = MD5.Create())
        //    {
        //        using (var stream = File.OpenRead(filename))
        //        {
        //            return Encoding.Default.GetString(md5.ComputeHash(stream));
        //        }
        //    }
        //}

        public static FileIndexItem ReadExifFromFile(FileIndexItem item)
        {

            var allExifItems = ImageMetadataReader.ReadMetadata(item.FilePath);

            foreach (var exifItem in allExifItems)
            {

                //Console.WriteLine(exifItem);

                var tCounts = exifItem.Tags.Count(p => p.DirectoryName == "IPTC" && p.Name == "Keywords");
                if (tCounts >= 1)
                {
                    item.Tags = exifItem.Tags.FirstOrDefault(p => p.DirectoryName == "IPTC" && p.Name == "Keywords")?.Description;
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


        public static IEnumerable<FileIndexItem> GetFiles(string subPath = "")
        {
            var path = AppSettingsProvider.BasePath + PathToSys(subPath);

            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in System.IO.Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].ToLower().EndsWith("jpg"))
                        {
                            var fileItem = new FileIndexItem
                            {
                                FilePath = files[i],
                                FileName = Path.GetFileName(files[i]),
                                FileHash = CalcHashCode(files[i]),
                                Folder = PathToUnixStyle(Path.GetDirectoryName(files[i]))
                            };
                            yield return fileItem;
                        }
                    }
                }
            }
        }

        public static string CalcHashCode(string filename)
        {
            FileStream stream = new FileStream(
                filename,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);

            try
            {
                return CalcHashCode(stream);
            }
            finally
            {
                stream.Close();
            }
        }

        public static string CalcHashCode(FileStream file)
        {
            //using (var stream = new BufferedStream(file, 1200000))
            //{
            //    SHA256Managed sha = new SHA256Managed();
            //    byte[] checksum = sha.ComputeHash(stream);
            //    return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            //}

            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            Byte[] hash = md5Provider.ComputeHash(file);
            var stringHash = Convert.ToBase64String(hash);

            stringHash = stringHash.Replace("/", "_");
            stringHash = stringHash.Replace("\\", "_");
            stringHash = stringHash.Replace("==", "");
            stringHash = stringHash.Replace("+", "0");

            return stringHash;

        }

        public static string PathToUnixStyle(string filepath)
        {
            filepath = filepath.Replace(AppSettingsProvider.BasePath, "");

            filepath = PathToSys(filepath);

            return filepath;
        }

        public static string PathToSys(string subPath)
        {
            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                subPath = subPath.Replace("\\", "/");
            }
            return subPath;
        }

        public static string PathToFull(string shortPath)
        {
            var filepath = AppSettingsProvider.BasePath + shortPath;

            filepath = PathToSys(filepath);

            return File.Exists(filepath) ? filepath : null;
        }
    }
}
