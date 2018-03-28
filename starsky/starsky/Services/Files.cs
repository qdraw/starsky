using System;
using System.Collections.Generic;
using System.IO;
using starsky.Models;

namespace starsky.Services
{
    public class Files
    {
        public static FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath = "")
        {
            var fullPath = FileIndexItem.DatabasePathToFilePath(subPath);

            if (!System.IO.Directory.Exists(fullPath) && System.IO.File.Exists(fullPath))
            {
                // file
                return FolderOrFileModel.FolderOrFileTypeList.File;
            }

            if (!System.IO.File.Exists(fullPath) && System.IO.Directory.Exists(fullPath))
            {
                // Directory
                return FolderOrFileModel.FolderOrFileTypeList.Folder;
            }

            return FolderOrFileModel.FolderOrFileTypeList.Deleted;
        }

        public static string[] GetAllFilesDirectory(string subPath = "")
        {
            var path = FileIndexItem.DatabasePathToFilePath(subPath);
            if (!System.IO.Directory.Exists(path)) return new List<string>().ToArray();
            string[] folders = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
            return folders;
        }

        public static string[] GetFilesInDirectory(string path, bool dbStyle = true)
        {
            if (path == null) return null;

            if (dbStyle)
            {
                path = FileIndexItem.DatabasePathToFilePath(path);
            }

            string[] allFiles = System.IO.Directory.GetFiles(path);

            var jpgFiles = new List<string>();
            foreach (var file in allFiles)
            {
                if (file.ToLower().EndsWith("jpg"))
                {
                    jpgFiles.Add(dbStyle ? FileIndexItem.FullPathToDatabaseStyle(file) : file);
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
                                FileHash = FileHash.GetHashCode(files[i]),
                                //Folder = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(files[i]))
                            };
                            yield return fileItem;
                        }
                    }
                }
            }
        }
        // end lagacy
        


    }
}
