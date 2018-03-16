﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        public string Get(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectItem> SearchObjectItem(string tag = "", int pageNumber = 0)
        {
            tag = tag.ToLower();

            if (pageNumber < 0)
            {
                pageNumber = pageNumber * -1;
            }

            var searchObjectItems = new List<ObjectItem>();

             var fileIndexQueryResults = _context.FileIndex.Where
                (p => p.Tags.Contains(tag)).ToList();

            var resultsInView = 50;

            var startIndex = (pageNumber * resultsInView);

            var endIndex = startIndex + resultsInView;
            if (endIndex >= fileIndexQueryResults.Count)
            {
                endIndex = fileIndexQueryResults.Count;
            }

            var i = startIndex;
            while (i < endIndex)
            {
                Console.WriteLine(i);

                var item = new ObjectItem();
                item.IsFolder = false;
                var file = fileIndexQueryResults[i];
                item.FilePath = file.FilePath;
                item.FileName = file.FileName;
                item.FileHash = file.FileHash;
                item.Tags = file.Tags;
                searchObjectItems.Add(item);
                i++;
            }

            //foreach (var file in fileIndexQueryResults)
            //{
            //    var item = new ObjectItem();
            //    item.IsFolder = false;
            //    item.FilePath = file.FilePath;
            //    item.FileName = file.FileName;
            //    item.FileHash = file.FileHash;
            //    item.Tags = file.Tags;

            //    searchObjectItems.Add(item);
            //}

            return searchObjectItems;
        }


        public List<FileIndexItem> GetAll(string subPath = "")
        {
            subPath = SubPathSlashRemove(subPath);
            return !string.IsNullOrEmpty(subPath) ?
                _context.FileIndex.Where
                    (p => !p.IsDirectory && p.ParentDirectory.Contains(subPath))
                    .OrderBy(r => r.FileName).ToList() :
                _context.FileIndex.Where(p => !p.IsDirectory).OrderBy(r => r.FileName).ToList();
        }


        public IEnumerable<string> GetChildFolders(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            if (subPath == "/")
            {
                subPath = "";
            }

            var childItemsInFolder = _context.FileIndex.Where(
                p => p.ParentDirectory.Contains(subPath)
            );

            var allSubFolders = childItemsInFolder.GroupBy(x => x.ParentDirectory, (key, group) => group.First());

            var directChildFolders = new HashSet<string>();
            foreach (var item in allSubFolders)
            {
                if (_getChildFolderByPath(item.ParentDirectory, subPath) != null)
                {
                    directChildFolders.Add(_getChildFolderByPath(item.ParentDirectory, subPath));
                }
            }
            return directChildFolders;
        }

        private string _getChildFolderByPath(string foldername, string subPath)
        {

            var itemSearch = Regex.Replace(foldername, @"^(" + subPath + @")", "", RegexOptions.IgnoreCase);
            itemSearch = SubPathSlashRemove(itemSearch);

            var slashesList = itemSearch.Split('/');

            if (slashesList.Length >= 1 && itemSearch != "")
            {
                if (subPath != "/")
                {
                    var childFolder = subPath + "/" + slashesList[0];
                    return childFolder;
                }
                else
                {
                    var childFolder = slashesList[0];
                    return childFolder;
                }

            }

            return null;

        }

        public string GetItemByHash(string path) {
            var query = _context.FileIndex.FirstOrDefault(p => p.FileHash == path);
            return query?.FilePath;
        }

        // Name of item by path
        public IEnumerable<ObjectItem> GetItem(string path = "")
        {
            var countDirectResults = _context.FileIndex.Count(p => p.FilePath == path);

            if (countDirectResults != 1) return null;

            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == path);

            var relativeObject = _getNextPrevInSubFolder(query?.ParentDirectory, path);

            var itemResultsList = new List<ObjectItem>();
            var itemResult = new ObjectItem
            {
                FilePath = query?.FilePath,
                FileName = query?.FileName,
                FileHash = query?.FileHash,
                RelativeObjects = relativeObject,
                Tags = query?.Tags
            };

            itemResultsList.Add(itemResult);
            return itemResultsList;
        }


        public RelativeObjects _getNextPrevInSubFolder(string parrentFolderPath, string fullImageFilePath)
        {
            var itemsInSubFolder = GetAll(parrentFolderPath).OrderBy(p => p.FileName).ToList();
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



        // complete files and folders
        public IEnumerable<ObjectItem> GetObjectItems(string subPath = "/")
        {
            var directItem = GetItem(subPath);
            if (directItem != null)
            {
                return directItem;
            }
            var files = GetFilesInFolder(subPath);
            var folders = GetChildFolders(subPath);
            var items = new List<ObjectItem>();

            foreach (var file in files)
            {
                var item = new ObjectItem();

                item.IsFolder = false;
                item.FilePath = file.FilePath;
                item.FileName = file.FileName;
                item.FileHash = file.FileHash;
                item.Tags = file.Tags;

                items.Add(item);
            }

            foreach (var folder in folders)
            {
                var item = new ObjectItem();
                item.IsFolder = true;
                item.FilePath = folder;
                item.FileName = folder;
                items.Add(item);
            }

            return new List<ObjectItem>(items.OrderBy(p => p.FilePath));

        }

        // List of files inside folder
        public IEnumerable<FileIndexItem> GetFilesInFolder(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);
            var content = _context.FileIndex.Where(
                p => p.ParentDirectory == subPath
            ).OrderBy(r => r.FileName).AsEnumerable();
            return content;
        }





        public string SubPathSlashRemove(string subPath = "/")
        {
            if (string.IsNullOrEmpty(subPath)) return subPath;

            // remove from end
            if (subPath.Substring(subPath.Length - 1, 1) == "/" && subPath != "/")
            {
                subPath = subPath.Substring(0, subPath.Length - 1);
            }

            // rm from begin
            if (subPath.Substring(0, 1) == "/" && subPath != "/")
            {
                subPath = subPath.Substring(1, subPath.Length - 1);
            }
            return subPath;
        }




        public IEnumerable<string> SyncFiles(string subPath = "")
        {

            var subFoldersFullPath = Files.GetAllFilesDirectory(subPath);



            // Delete old folders from database
            var subFoldersDbStyle = new List<string>();

            var databaseFolderList = _context.FileIndex.Where(p => p.IsDirectory).ToList();
            foreach (var foldersFullPath in subFoldersDbStyle)
            {
                subFoldersDbStyle.Add(FileIndexItem.FullPathToDatabaseStyle(foldersFullPath));
            }

            IEnumerable<string> differenceFolders = databaseFolderList.Select(item => item.FilePath).Except(subFoldersDbStyle);

            // Remove items that are removed from file sytem
            foreach (var item in differenceFolders)
            {
                var ditem = databaseFolderList.FirstOrDefault(p => p.FilePath == item && p.IsDirectory);
                RemoveItem(ditem);
                Console.Write("`");
            }

            subFoldersDbStyle = new List<string>();
            databaseFolderList = new List<FileIndexItem>();


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
                    AddItem(folderItem);
                    // We dont need this localy
                }
                // end folder


                // List all localy
                var databaseFileList = GetAll(FileIndexItem.FullPathToDatabaseStyle(singleFolderFullPath));

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
                        var databaseItem = Exif.ReadExifFromFile(filesInDirectoryFullPath[i]);
                        databaseItem.AddToDatabase = DateTime.Now;
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


        //var localFileList = Files.GetFiles(subPath).ToList();
        //var databaseFileList = GetAll(subPath);


        //// Check for updated files based on hash
        //var localFileListFileHash = localFileList.Select(item => item.FileHash).ToList();
        //var databaseFileListFileHash =
        //    databaseFileList.Select(item => item.FileHash).ToList();

        //Console.Write(" . . ");

        //IEnumerable<string> differenceFileHash = databaseFileListFileHash.Except(localFileListFileHash);

        //Console.Write(" .. ");

        //foreach (var item in differenceFileHash)
        //{
        //    var ditem = databaseFileList.FirstOrDefault(p => p.FileHash == item);
        //    databaseFileList.Remove(ditem);
        //    RemoveItem(ditem);
        //    Console.Write("^");
        //}

        // temp off
        //localFileList.ForEach(item =>
        //{
        //    var localItem = item;

        //    var dbMatchFirst = databaseFileList
        //        .FirstOrDefault(p => p.FilePath == Files.PathToUnixStyle(localItem.FilePath)
        //        && p.FileHash == localItem.FileHash);


        //    if (dbMatchFirst == null)
        //    {
        //        Console.Write("_");

        //        item.AddToDatabase = DateTime.Now;
        //        item = Exif.ReadExifFromFile(item);

        //        item.FilePath = item. Files.PathToUnixStyle(item.FilePath);
        //        AddItem(item);
        //        databaseFileList.Add(item);
        //    }

        //});

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
