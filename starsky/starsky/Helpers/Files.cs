using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using starsky.Models;

namespace starsky.Helpers
{
    public static class Files
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
            // This one does not include the subfolder itself
            var path = FileIndexItem.DatabasePathToFilePath(subPath);

            if (!Directory.Exists(path)) return new List<string>().ToArray();
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            // Used For subfolders

            return folders;
        }

        // Returns a list of Files in a directory (non-recruisive)
        public static string[] GetFilesInDirectory(string path, bool dbStyle = true)
        {
            if (path == null) return Enumerable.Empty<string>().ToArray();

            if (dbStyle)
            {
                path = FileIndexItem.DatabasePathToFilePath(path);
            }
            string[] allFiles = Directory.GetFiles(path);
            
            // Used for jpeg files

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
        
        public enum ImageFormat
        {
            bmp,
            jpg,
            gif,
            tiff,
            png,
            unknown,
            notfound
        }

     
        public static ImageFormat GetImageFormat(string filePath)
        {
            if (!File.Exists(filePath)) return ImageFormat.notfound;
            
            byte[] buffer = new byte[512];
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return GetImageFormat(buffer);
        }

        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp    = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif    = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png    = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff   = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2  = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg   = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2  = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpg;

            return ImageFormat.unknown;
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException) 
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
        
        // Read a folder recruisive
        public static IEnumerable<string> GetFilesRecrusive(string subPath = "")
        {
            var path = FileIndexItem.DatabasePathToFilePath(subPath);

            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
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
                            yield return files[i];
                        }
                    }
                }
            }
        }
        


    }
}
