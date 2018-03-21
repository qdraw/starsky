using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using starsky.Interfaces;
using starsky.Models;
using starsky.Data;


namespace starsky.Services
{
    public partial class Query : IUpdate
    {
        private readonly ApplicationDbContext _context;

        public Query(ApplicationDbContext context)
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


        public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            var queryItems = _context.FileIndex
                .Where(p => p.ParentDirectory == subPath)
                .OrderBy(p => p.FileName).ToList();

            if (!queryItems.Any())
            {
                return new List<FileIndexItem>();
            }

            // temp feature to hide deleted items
            var displayItems = new List<FileIndexItem>();
            foreach (var item in queryItems)
            {
                if (item.Tags == null)
                {
                    item.Tags = string.Empty;
                }

                if (!item.Tags.Contains("<delete>"))
                {
                    displayItems.Add(item);
                }
            }
            // temp feature to hide deleted items

            return displayItems;
        }


        public string SubPathSlashRemove(string subPath = "/")
        {
            if (string.IsNullOrEmpty(subPath)) return subPath;

            // remove / from end
            if (subPath.Substring(subPath.Length - 1, 1) == "/" && subPath != "/")
            {
                subPath = subPath.Substring(0, subPath.Length - 1);
            }

            return subPath;
        }



       

        //public IEnumerable<string>
        //    RemoveEmptyFolders(string subPath)
        //{
        //    // You will get Out of Memory issues
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
