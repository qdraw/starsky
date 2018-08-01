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
        public static FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string fullFilePath = "")
        {

            if (!Directory.Exists(fullFilePath) && File.Exists(fullFilePath))
            {
                // file
                return FolderOrFileModel.FolderOrFileTypeList.File;
            }

            if (!File.Exists(fullFilePath) && Directory.Exists(fullFilePath))
            {
                // Directory
                return FolderOrFileModel.FolderOrFileTypeList.Folder;
            }

            return FolderOrFileModel.FolderOrFileTypeList.Deleted;
        }

        // Returns a list of directories
        public static string[] GetAllFilesDirectory(string fullFilePath = "")
        {

            if (!Directory.Exists(fullFilePath)) return new List<string>().ToArray();
            string[] folders = Directory.GetDirectories(fullFilePath, "*", SearchOption.AllDirectories);
            // Used For subfolders

            return folders;
        }

        // Returns a list of Files in a directory (non-recruisive)
        public static string[] GetFilesInDirectory(string fullFilePath, AppSettings appSettings)
        {
            if (fullFilePath == null) return Enumerable.Empty<string>().ToArray();

            string[] allFiles = Directory.GetFiles(fullFilePath);

            var imageFilesList = new List<string>();
            foreach (var file in allFiles)
            {
                var extension = Path.GetExtension(file).ToLower().Replace(".",string.Empty);
                // Path.GetExtension uses (.ext)
                if (ExtensionList.Contains(extension))
                {
                    imageFilesList.Add(file);
                }
            }

            return imageFilesList.ToArray();
        }

        public enum ImageFormat
        {
            notfound = -1,
            unknown = 0,

            // Viewable types
            jpg = 10,
            tiff = 12,
            bmp = 13,
            gif = 14,
            png = 15,

            // Sitecare files
            xmp = 30
        }

        private static readonly List<string> Extensionjpg = new List<string> {"jpg", "jpeg"};
        private static readonly List<string> Extensiontiff = new List<string> {"tiff", "arw", "dng" };
        private static readonly List<string> Extensionbmp = new List<string> {"bmp"};
        private static readonly List<string> Extensiongif = new List<string> {"gif"};
        private static readonly List<string> Extensionpng = new List<string> {"png"};

        public static List<string> ExtensionList
        {
            get
            {
                var extensionList = new List<string>();
                extensionList.AddRange(Extensionjpg);
                extensionList.AddRange(Extensiontiff);
                extensionList.AddRange(Extensionbmp);
                extensionList.AddRange(Extensiongif);
                extensionList.AddRange(Extensionpng);
                return extensionList;
            }
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
            var xmp    = Encoding.ASCII.GetBytes("<x:xmpmeta");    // xmp

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
            
            if (xmp.SequenceEqual(bytes.Take(xmp.Length)))
                return ImageFormat.xmp;

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
        
        public static IEnumerable<string> GetFilesRecrusive(string fullFilePath)
        {
            List<string> findlist = new List<string>();

            /* I begin a recursion, following the order:
             * - Insert all the files in the current directory with the recursion
             * - Insert all subdirectories in the list and rebegin the recursion from there until the end
             */
            RecurseFind( fullFilePath, findlist );

            return findlist;
        }

        private static void RecurseFind( string path, List<string> list )
        {
            string[] fl = Directory.GetFiles(path);
            string[] dl = Directory.GetDirectories(path);
            if ( fl.Length>0 || dl.Length>0 )
            {
                //I begin with the files, and store all of them in the list
                foreach(string s in fl)
                    list.Add(s);
                //I then add the directory and recurse that directory, the process will repeat until there are no more files and directories to recurse
                foreach(string s in dl)
                {
                    list.Add(s);
                    RecurseFind(s, list);
                }
            }
        }

    }
}
