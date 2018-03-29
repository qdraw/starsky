using System.Collections.Generic;
using System.IO;
using starsky.Models;

namespace starsky.Services
{
    public class Files
    {
        
        // is the subpath a folder or file
        public static FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath = "")
        {
            var fullPath = FileIndexItem.DatabasePathToFilePath(subPath);

            if (!Directory.Exists(fullPath) && File.Exists(fullPath))
            {
                // file
                return FolderOrFileModel.FolderOrFileTypeList.File;
            }

            if (!File.Exists(fullPath) && Directory.Exists(fullPath))
            {
                // Directory
                return FolderOrFileModel.FolderOrFileTypeList.Folder;
            }

            return FolderOrFileModel.FolderOrFileTypeList.Deleted;
        }

        // Returns a list of directories
        public static string[] GetAllFilesDirectory(string subPath = "")
        {
            var path = FileIndexItem.DatabasePathToFilePath(subPath);
            if (!Directory.Exists(path)) return new List<string>().ToArray();
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            return folders;
        }

        // Returns a list of Files in a directory (non-recruisive)
        public static string[] GetFilesInDirectory(string path, bool dbStyle = true)
        {
            if (path == null) return null;

            if (dbStyle)
            {
                path = FileIndexItem.DatabasePathToFilePath(path);
            }
            string[] allFiles = Directory.GetFiles(path);

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
        // Read a folder recruisive
        // Currently we dont use it because memory problems on linux-arm
//        public static IEnumerable<FileIndexItem> GetFilesRecrusive(string subPath = "")
//        {
//            var path = FileIndexItem.DatabasePathToFilePath(subPath);
//
//            Queue<string> queue = new Queue<string>();
//            queue.Enqueue(path);
//            while (queue.Count > 0)
//            {
//                path = queue.Dequeue();
//                try
//                {
//                    foreach (string subDir in System.IO.Directory.GetDirectories(path))
//                    {
//                        Console.WriteLine(subDir);
//                        queue.Enqueue(subDir);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.Error.WriteLine(ex);
//                }
//                string[] files = null;
//                try
//                {
//                    files = System.IO.Directory.GetFiles(path);
//                }
//                catch (Exception ex)
//                {
//                    Console.Error.WriteLine(ex);
//                }
//                if (files != null)
//                {
//                    for (int i = 0; i < files.Length; i++)
//                    {
//                        if (files[i].ToLower().EndsWith("jpg"))
//                        {
//                            var fileItem = new FileIndexItem
//                            {
//                                FilePath = files[i],
//                                FileName = Path.GetFileName(files[i]),
//                                FileHash = FileHash.GetHashCode(files[i]),
//                                //Folder = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(files[i]))
//                            };
//                            yield return fileItem;
//                        }
//                    }
//                }
//            }
//        }        // end lagacy
        


    }
}
