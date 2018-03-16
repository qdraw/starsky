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
        public static string[] GetAllFilesDirectory(string subPath = "")
        {
            var path = FileIndexItem.DatabasePathToFilePath(subPath);
            string[] folders = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
            return folders;
        }

        public static string[] GetFilesInDirectory(string folderFullPath)
        {
            string[] allFiles = System.IO.Directory.GetFiles(folderFullPath);

            var jpgFiles = new List<string>();
            foreach (var file in allFiles)
            {
                if (file.ToLower().EndsWith("jpg"))
                {
                    jpgFiles.Add(file);
                }

            }
            return jpgFiles.ToArray();
        }

        
        // Legacy Keep here

        public static IEnumerable<FileIndexItem> GetFilesRecrusive(string subPath = "")
        {
            var path = FileIndexItem.DatabasePathToFilePath(subPath);

            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in System.IO.Directory.GetDirectories(path))
                    {
                        Console.WriteLine(subDir);
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
                                FileHash = FileHash.CalcHashCode(files[i]),
                                //Folder = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(files[i]))
                            };
                            yield return fileItem;
                        }
                    }
                }
            }
        }

        


    }
}
