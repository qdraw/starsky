using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using starsky.Interfaces;
using starsky.Models;
using starsky.Data;
using starsky.ViewModels;


namespace starsky.Services
{
    public class SqlUpdateStatus : IUpdate
    {
        private readonly ApplicationDbContext _context;

        public SqlUpdateStatus(ApplicationDbContext context)
        {
            _context = context;
        }

 
        public List<FileIndexItem> GetAllFiles(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            return _context.FileIndex.Where
                    (p => !p.IsDirectory && p.ParentDirectory.Contains(subPath))
                .OrderBy(r => r.FileName).ToList();
        }

        private const int ResultsInView = 50;

        public IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0)
        {
            tag = tag.ToLower();

            if (pageNumber < 0)
            {
                pageNumber = pageNumber * -1;
            }

            var searchObjectItems = new List<FileIndexItem>();

            var fileIndexQueryResults = _context.FileIndex.Where
               (p => !p.IsDirectory && p.Tags.Contains(tag)).OrderByDescending(p => p.DateTime).ToList();

            var startIndex = (pageNumber * ResultsInView);

            var endIndex = startIndex + ResultsInView;
            if (endIndex >= fileIndexQueryResults.Count)
            {
                endIndex = fileIndexQueryResults.Count;
            }

            var i = startIndex;
            while (i < endIndex)
            {
                searchObjectItems.Add(fileIndexQueryResults[i]);
                i++;
            }

            return searchObjectItems;
        }

        public int SearchLastPageNumber(string tag)
        {
            tag = tag.ToLower();
            var fileIndexQueryCount = _context.FileIndex.Count
                (p => !p.IsDirectory && p.Tags.Contains(tag));

            var searchLastPageNumbers = (fileIndexQueryCount / ResultsInView) -1;

            if (fileIndexQueryCount <= ResultsInView)
            {
                searchLastPageNumbers = 0;
            }

            return searchLastPageNumbers;
        }







        public string GetItemByHash(string fileHash)
        {
            var query = _context.FileIndex.FirstOrDefault(p => p.FileHash == fileHash);
            return query?.FilePath;
        }

        //// Name of item by path
        public ObjectItem SingleItem(string singleItemDbPath)
        {
            if (string.IsNullOrWhiteSpace(singleItemDbPath)) return null;

            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == singleItemDbPath && !p.IsDirectory);

            if (query == null) return null;

            var relativeObject = _getNextPrevInSubFolder(query?.ParentDirectory, singleItemDbPath);

            var itemResult = new ObjectItem
            {
                FileIndexItem = query,
                RelativeObjects = relativeObject
            };

            return itemResult;
        }


        public RelativeObjects _getNextPrevInSubFolder(string parrentFolderPath, string fullImageFilePath)
        {
            var itemsInSubFolder = GetAllFiles(parrentFolderPath).OrderBy(p => p.FileName).ToList();
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == fullImageFilePath);

            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1)
            {
                relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FilePath;
            }

            if (photoIndexOfSubFolder != 0)
            {
                relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FilePath;
            }
            return relativeObject;
        }




        public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            return _context.FileIndex.Where(p => p.ParentDirectory == subPath).OrderBy(p => p.FileName).AsEnumerable();
        }


        public string SubPathSlashRemove(string subPath = "/")
        {
            if (string.IsNullOrEmpty(subPath)) return subPath;

            // remove from end
            if (subPath.Substring(subPath.Length - 1, 1) == "/" && subPath != "/")
            {
                subPath = subPath.Substring(0, subPath.Length - 1);
            }

            return subPath;
        }




        public IEnumerable<string> SyncFiles(string subPath = "")
        {

            var subFoldersFullPath = Files.GetAllFilesDirectory(subPath).ToList();



            // Delete old folders from database
            var subFoldersDbStyle = new List<string>();

            var databaseFolderList = _context.FileIndex.Where(p => p.IsDirectory).ToList();

            foreach (var foldersFullPath in databaseFolderList)
            {
                subFoldersDbStyle.Add(FileIndexItem.FullPathToDatabaseStyle(foldersFullPath.FilePath));
            }



            // Add the subpath to the database  => later on dont delete this
            var subPathItem = new FileIndexItem()
            {
                AddToDatabase = DateTime.UtcNow,
                FilePath = subPath,
                FileName = subPath, // test
                IsDirectory = true,
                ParentDirectory = FileIndexItem.FullPathToDatabaseStyle(
                    Path.GetDirectoryName(FileIndexItem.DatabasePathToFilePath(subPath)))
            };
            if (string.IsNullOrWhiteSpace(subPathItem.ParentDirectory))
            {
                subPathItem.ParentDirectory = "/";
            }

            var ditem1 = databaseFolderList.FirstOrDefault(p => p.FilePath == subPath && p.IsDirectory);
            if (ditem1 == null)
            {
                AddItem(subPathItem);
            }
            // end

            IEnumerable<string> differenceFolders = databaseFolderList.Select(item => item.FilePath).Except(subFoldersDbStyle);

            // Remove items that are removed from file sytem
            foreach (var item in differenceFolders)
            {
                var ditem = databaseFolderList.FirstOrDefault(p => p.FilePath == item && p.IsDirectory);
                if (ditem?.FilePath == subPath) continue;
                // dont remove the direct subpath
                RemoveItem(ditem);
                Console.Write("`");
            }

            subFoldersDbStyle = new List<string>();


            foreach (var singleFolderFullPath in subFoldersFullPath)
            {

                // Check if Directory is in database
                var dbFolderMatchFirst = _context.FileIndex.FirstOrDefault(p =>
                    p.IsDirectory && p.FilePath == FileIndexItem.FullPathToDatabaseStyle(singleFolderFullPath));


                if (dbFolderMatchFirst == null)
                {
                    var folderItem = new FileIndexItem();
                    folderItem.FilePath = FileIndexItem.FullPathToDatabaseStyle(singleFolderFullPath);
                    folderItem.IsDirectory = true;
                    folderItem.AddToDatabase = DateTime.UtcNow;
                    folderItem.FileName = FileIndexItem.FullPathToDatabaseStyle(Path.GetFileName(singleFolderFullPath));
                    folderItem.ParentDirectory = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(singleFolderFullPath));
                    AddItem(folderItem);
                    // We dont need this localy
                }
                // end folder
                

                // List all localy
                var databaseFileList = GetAllFiles(FileIndexItem.FullPathToDatabaseStyle(singleFolderFullPath));

                string[] filesInDirectoryFullPath = Files.GetFilesInDirectory(singleFolderFullPath);
                var localFileListFileHash = FileHash.CalcHashCode(filesInDirectoryFullPath);

                var databaseFileListFileHash =
                    databaseFileList.Select(item => item.FileHash).ToList();

                // Compare to delete
                IEnumerable<string> differenceFileHash = databaseFileListFileHash.Except(localFileListFileHash);

                // Remove items from database that are removed from file system
                foreach (var item in differenceFileHash)
                {
                    var ditem = databaseFileList.FirstOrDefault(p => p.FileHash == item && !p.IsDirectory);
                    databaseFileList.Remove(ditem);
                    RemoveItem(ditem);
                    Console.Write("^");
                }
                differenceFileHash = new List<string>();

                // Add new items to database

                for (int i = 0; i < filesInDirectoryFullPath.Length; i++)
                {
                    var dbMatchFirst = databaseFileList
                        .FirstOrDefault(p => p.FilePath == FileIndexItem.FullPathToDatabaseStyle(filesInDirectoryFullPath[i])
                                             && p.FileHash == localFileListFileHash[i]);
                    if (dbMatchFirst == null)
                    {
                        Console.Write("_");
                        var databaseItem = ExifRead.ReadExifFromFile(filesInDirectoryFullPath[i]);
                        databaseItem.AddToDatabase = DateTime.UtcNow;
                        databaseItem.FileHash = localFileListFileHash[i];
                        databaseItem.FileName = Path.GetFileName(filesInDirectoryFullPath[i]);
                        databaseItem.IsDirectory = false;
                        databaseItem.ParentDirectory = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(filesInDirectoryFullPath[i]));
                        databaseItem.FilePath = FileIndexItem.FullPathToDatabaseStyle(filesInDirectoryFullPath[i]);
                        AddItem(databaseItem);
                        databaseFileList.Add(databaseItem);
                    }
                }
                

            }

            return null;
        }

        // Todo: do i need this:?

        ////Check fileName Difference
        //var localFileListFileName = localFileList.OrderBy(r => r.FileName)
        //    .Select(item => Files.PathToUnixStyle(item.FilePath)).ToList();
        //var databaseFileListFileName =
        //    databaseFileList.OrderBy(r => r.FileName).Select(item => item.FilePath).ToList();

        //IEnumerable<string> differenceFileNames = databaseFileListFileName.Except(localFileListFileName);

        //foreach (var item in differenceFileNames)
        //{
        //    Console.Write("*");

        //    var ditem = databaseFileList.FirstOrDefault(p => p.FilePath == item);
        //    databaseFileList.Remove(ditem);
        //    RemoveItem(ditem);
        //}

        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
            _context.Attach(updateStatusContent).State = EntityState.Modified;
            _context.SaveChanges();
            return updateStatusContent;
        }


        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {
            _context.FileIndex.Add(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }

        public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
        {
            _context.FileIndex.Remove(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }

        
    }
}
