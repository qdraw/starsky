using System;
using System.Collections.Generic;
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

        public List<FileIndexItem> GetAll(string subPath = "")
        {
            return _context.FileIndex.Where(p => p.FilePath.Contains(subPath)).OrderBy(r => r.FileName).ToList();
        }

        //public IEnumerable<string> GetAll()
        //{
        //    var dbItems = _context.FileIndex.OrderBy(r => r.FileName);
        //    return dbItems.Select(item => item.FilePath).ToList();
        //}

        //public IEnumerable<string> RemoveOldFilesByFileList(IEnumerable<string> shortFileList)
        //{
        //    var newFileList = new List<string>();
        //    foreach (var item in shortFileList)
        //    {
        //        if (Files.PathToFull(item) == null)
        //        {
        //            var firstOrDefault = _context.FileIndex.FirstOrDefault(r => r.FilePath == item);
        //            if (firstOrDefault != null)
        //            {
        //                _context.Remove(firstOrDefault);
        //            }
        //        }
        //        else
        //        {
        //            newFileList.Add(item);
        //        }
        //    }

        //    return newFileList;
        //}

        public IEnumerable<string> GetChildFolders(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            if (subPath == "/")
            {
                subPath = "";
            }

            var childItemsInFolder = _context.FileIndex.Where(
                p => p.Folder.Contains(subPath)
            );

            var allSubFolders = childItemsInFolder.GroupBy(x => x.Folder, (key, group) => group.First());

            var directChildFolders = new HashSet<string>();
            foreach (var item in allSubFolders)
            {
                if (GetChildFolders(item.Folder, subPath) != null)
                {
                    directChildFolders.Add(GetChildFolders(item.Folder, subPath));
                }
            }
            return directChildFolders;
        }

        public string GetChildFolders(string item, string subPath)
        {
            
            var itemSearch = Regex.Replace(item, @"^(" + subPath + @")", "", RegexOptions.IgnoreCase);
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

        public IEnumerable<ObjectItem> GetItem(string path = "")
        {
            var countDirectResults = _context.FileIndex.Count(p => p.FilePath == path);

            if (countDirectResults != 1) return null;

            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == path);


            var itemResultsList = new List<ObjectItem>();
            var itemResult = new ObjectItem
            {
                FilePath = query?.FilePath,
                FileName = query?.FileName,
                FileHash = query?.FileHash,
                Tags = query?.Tags
            };

            itemResultsList.Add(itemResult);
            return itemResultsList;

        }



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


        public IEnumerable<FileIndexItem> GetFilesInFolder(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);
            var content = _context.FileIndex.Where(
                p => p.Folder == subPath
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

            var localFileList = Files.GetFiles(subPath).ToList();
            var databaseFileList = GetAll(subPath);

            // Check for updated files based on hash
            var localFileListFileHash = localFileList.OrderBy(r => r.FileHash).Select(item => item.FileHash).ToList();
            var databaseFileListFileHash =
                databaseFileList.OrderBy(r => r.FileHash).Select(item => item.FileHash).ToList();

            Console.Write(".");

            IEnumerable<string> differenceFileHash = databaseFileListFileHash.Except(localFileListFileHash);

            foreach (var item in differenceFileHash)
            {
                var ditem = databaseFileList.FirstOrDefault(p => p.FileHash == item);
                databaseFileList.Remove(ditem);
                RemoveItem(ditem);
                Console.Write("^");
            }


            localFileList.ForEach(item =>
            {
                var localItem = item;
                var dbMatchFirst = _context.FileIndex
                    .FirstOrDefault(p => p.FilePath == Files.PathToUnixStyle(localItem.FilePath)
                                         && p.FileHash == localItem.FileHash);
                Console.Write("_");

                if (dbMatchFirst == null)
                {
                    item.AddToDatabase = DateTime.Now;
                    item = Files.ReadExifFromFile(item);

                    item.FilePath = Files.PathToUnixStyle(item.FilePath);
                    AddItem(item);
                    databaseFileList.Add(item);
                }

            });

            //Check fileName Difference
            var localFileListFileName = localFileList.OrderBy(r => r.FileName)
                .Select(item => Files.PathToUnixStyle(item.FilePath)).ToList();
            var databaseFileListFileName =
                databaseFileList.OrderBy(r => r.FileName).Select(item => item.FilePath).ToList();

            IEnumerable<string> differenceFileNames = databaseFileListFileName.Except(localFileListFileName);

            foreach (var item in differenceFileNames)
            {
                Console.Write("*");

                var ditem = databaseFileList.FirstOrDefault(p => p.FilePath == item);
                databaseFileList.Remove(ditem);
                RemoveItem(ditem);
            }

            return null;
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



