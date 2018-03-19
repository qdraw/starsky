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
    public partial class SqlUpdateStatus : IUpdate
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
                    (p => !p.IsDirectory && p.ParentDirectory == subPath) // used to be contains
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





        public FileIndexItem GetObjectByFilePath(string filePath)
        {
            filePath = SubPathSlashRemove(filePath);
            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
            return query;
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

        public List<string> RenameListItemsToDbStyle(List<string> localSubFolderList)
        {
            var localSubFolderListDatabaseStyle = new List<string>();

            foreach (var item in localSubFolderList)
            {
                localSubFolderListDatabaseStyle.Add(FileIndexItem.FullPathToDatabaseStyle(item));
            }

            return localSubFolderListDatabaseStyle;
        }

        public List<FileIndexItem> RemoveOldFilePathItemsFromDatabase(
            List<string> localSubFolderListDatabaseStyle,
            List<FileIndexItem> databaseSubFolderList,
            string subpath
            )
        {

            //Check fileName Difference
            var databaseFileListFileName =
                databaseSubFolderList.Where
                    (p => p.FilePath.Contains(subpath))
                    .OrderBy(r => r.FileName)
                    .Select(item => item.FilePath)
                    .ToList();

            IEnumerable<string> differenceFileNames = databaseFileListFileName.Except(localSubFolderListDatabaseStyle);

            Console.Write(differenceFileNames.Count() + " "  + databaseSubFolderList.Count);

            // Delete removed items
            foreach (var item in differenceFileNames)
            {
                Console.Write("*");

                var ditem = databaseSubFolderList.FirstOrDefault(p => p.FilePath == item);
                databaseSubFolderList.Remove(ditem);
                RemoveItem(ditem);

                // if directory remove parent elements
                // 1. Index all folders
                // 2. Rename single folder
                // 3. The files are keeped in the index
                if (ditem?.IsDirectory == null) continue;
                if (!ditem.IsDirectory) continue;

                var orphanPictures =_context.FileIndex.Where(p => !p.IsDirectory && p.ParentDirectory == ditem.FilePath);
                foreach (var orphanItem in orphanPictures)
                {
                    Console.Write("$");
                    RemoveItem(orphanItem);
                }


            }

            return databaseSubFolderList;
        }

        //public IEnumerable<string>
        //    RemoveEmptyFolders(string subPath)
        //{

        //    // 1. Index all folders
        //    // 2. Rename single folder
        //    // 3. The files are keeped in the index

        //    var allItemsInDb = _context.FileIndex.Where
        //        (p => p.ParentDirectory.Contains(subPath))
        //        .OrderBy(r => r.FileName).ToList();

        //    foreach (var dbItem in allItemsInDb)
        //    {
        //        if (!dbItem.IsDirectory)
        //        {
        //            var res = allItemsInDb.Where(
        //                p =>
        //                    p.IsDirectory &&
        //                    p.FilePath == dbItem.ParentDirectory
        //            );
        //            if (!res.Any())
        //            {
        //                var c = res.Count();
        //                var q = dbItem.FilePath;
        //                var w = dbItem.IsDirectory;
        //                RemoveItem(dbItem);
        //            }
        //        }

        //    }

        //    return null;
        //}

        public void AddFoldersToDatabase(List<string> localSubFolderDbStyle, List<FileIndexItem> databaseSubFolderList)
        {
            foreach (var singleFolderDbStyle in localSubFolderDbStyle)
            {

                // Check if Directory is in database
                var dbFolderMatchFirst = _context.FileIndex.FirstOrDefault(p =>
                        p.IsDirectory &&
                        p.FilePath == singleFolderDbStyle
                    );

                // Folders!!!!
                if (dbFolderMatchFirst == null)
                {
                    var folderItem = new FileIndexItem();
                    folderItem.FilePath = singleFolderDbStyle;
                    folderItem.IsDirectory = true;
                    folderItem.AddToDatabase = DateTime.UtcNow;
                    folderItem.FileName = singleFolderDbStyle.Split("/").LastOrDefault();
                    folderItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(singleFolderDbStyle).LastOrDefault();
                    AddItem(folderItem);
                    // We dont need this localy
                }

                // end folder
            }
        }

        public void AddPhotoToDatabase(
            List<string> localSubFolderDbStyle,
            List<FileIndexItem> databaseFileList
            )
        {
            foreach (var singleFolderDbStyle in localSubFolderDbStyle)
            {

                // Check if Photo is in database
                var dbFolderMatchFirst = databaseFileList.FirstOrDefault(
                    p =>
                        !p.IsDirectory &&
                        p.FilePath == singleFolderDbStyle
                    );

                // Console.WriteLine(singleFolderDbStyle);

                if (dbFolderMatchFirst == null)
                {
                    // photo
                    Console.Write(".");
                    var singleFilePath = FileIndexItem.DatabasePathToFilePath(singleFolderDbStyle);
                    var databaseItem = ExifRead.ReadExifFromFile(singleFilePath);
                    databaseItem.AddToDatabase = DateTime.UtcNow;
                    databaseItem.FileHash = FileHash.CalcHashCode(singleFilePath);
                    databaseItem.FileName = Path.GetFileName(singleFilePath);
                    databaseItem.IsDirectory = false;
                    databaseItem.ParentDirectory = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(singleFilePath));
                    databaseItem.FilePath = singleFolderDbStyle;

                    AddItem(databaseItem);
                    databaseFileList.Add(databaseItem);
                }

                // end folder
            }
        }

        public void AddSubPathFolder(string subPath)
        {
            subPath = SubPathSlashRemove(subPath);

            var listOfSubpaths = Breadcrumbs.BreadcrumbHelper(subPath);
            listOfSubpaths.Add(subPath);

            foreach (var itemSubpath in listOfSubpaths)
            {
                var countFolder = _context.FileIndex.Count(p => p.FilePath == itemSubpath);
                if (countFolder == 0)
                {
                    var newItem = new FileIndexItem();
                    newItem.FilePath = itemSubpath;
                    newItem.IsDirectory = true;
                    if (itemSubpath != "/")
                    {
                        newItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(itemSubpath).LastOrDefault();
                    }
                    newItem.FileName = itemSubpath.Split("/").LastOrDefault();
                    AddItem(newItem);
                }
            }
        }

        public void CheckMd5Hash(
            List<string> localSubFolderDbStyle,
            List<FileIndexItem> databaseFileList
            )
        {
            foreach (var itemLocal in localSubFolderDbStyle)
            {
                var dbItem = databaseFileList.FirstOrDefault(p => p.FilePath == itemLocal);
                if (dbItem != null)
                {
                    // Check if Hash is changed
                    var localHash = FileHash.CalcHashCode(FileIndexItem.DatabasePathToFilePath(itemLocal));
                    if (localHash != dbItem.FileHash)
                    {
                        RemoveItem(dbItem);
                        var updatedDatabaseItem = ExifRead.ReadExifFromFile(FileIndexItem.DatabasePathToFilePath(itemLocal));
                        updatedDatabaseItem.FilePath = dbItem.FilePath;
                        updatedDatabaseItem.FileHash = localHash;
                        updatedDatabaseItem.FileName = dbItem.FileName;
                        updatedDatabaseItem.AddToDatabase = DateTime.Now;
                        updatedDatabaseItem.IsDirectory = false;
                        updatedDatabaseItem.ParentDirectory = dbItem.ParentDirectory;
                        AddItem(updatedDatabaseItem);
                    }
                }

            }

        }

        public IEnumerable<string> SyncFiles(string subPath = "")
        {
            SyncDeleted(subPath);
            SyncSingleFile(subPath);

            // if folder: 
            var localSubFolderDbStyle = RenameListItemsToDbStyle(
                Files.GetAllFilesDirectory(subPath).ToList()
            );


            var databaseSubFolderList = _context.FileIndex.Where(p => p.IsDirectory).ToList();

            // Sync for folders
            RemoveOldFilePathItemsFromDatabase(localSubFolderDbStyle, databaseSubFolderList,subPath);
            AddFoldersToDatabase(localSubFolderDbStyle, databaseSubFolderList);

            Console.WriteLine(".");

            // Allow sync for direct folder
            localSubFolderDbStyle.Add(subPath);

            foreach (var singleFolder in localSubFolderDbStyle)
            {
                Console.Write(singleFolder + "  ");

                var databaseFileList = GetAllFiles(singleFolder);
                var localFarrayFilesDbStyle = Files.GetFilesInDirectory(singleFolder).ToList();

                databaseFileList = RemoveOldFilePathItemsFromDatabase(localFarrayFilesDbStyle, databaseFileList, subPath);
                CheckMd5Hash(localFarrayFilesDbStyle, databaseFileList);
                AddPhotoToDatabase(localFarrayFilesDbStyle, databaseFileList);
                Console.WriteLine("-");
            }

            AddSubPathFolder(subPath);

            return null;
        }


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
