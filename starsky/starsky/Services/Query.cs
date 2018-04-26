using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using starsky.Interfaces;
using starsky.Models;
using starsky.Data;
using starsky.Helpers;


namespace starsky.Services
{
    public partial class Query : IQuery
    {
        private readonly ApplicationDbContext _context;

        public Query(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get a list of all files inside an folder
        // But this uses a database as source
        public List<FileIndexItem> GetAllFiles(string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);

            return _context.FileIndex.Where
                    (p => !p.IsDirectory && p.ParentDirectory == subPath)
                .OrderBy(r => r.FileName).ToList();
        }
        
        // Includes sub items in file
        // Used for Orphan Check
        // All files in
        public List<FileIndexItem> GetAllRecursive(
            string subPath = "/")
        {
            subPath = SubPathSlashRemove(subPath);
            
            return (from a in _context.FileIndex
                where (a.ParentDirectory ?? "").Contains(subPath) || (a.ParentDirectory ?? "").Contains(subPath)
                select a).OrderBy(r => r.FileName).ToList();
        }

        // Return database object file or folder
        public FileIndexItem GetObjectByFilePath(string filePath)
        {
            filePath = SubPathSlashRemove(filePath);
            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == filePath);
            return query;
        }

        // Return a File Item By it Hash value
        // New added, directory hash now also hashes
        public string GetItemByHash(string fileHash)
        {
            var query = _context.FileIndex.FirstOrDefault(
                p => p.FileHash == fileHash 
                     && !p.IsDirectory
             );
            return query?.FilePath;
        }


        // Remove the '/' from the end of the url
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


     
     
      

        // Currently not in use.
        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
            _context.Attach(updateStatusContent).State = EntityState.Modified;
            _context.SaveChanges();
            return updateStatusContent;
        }

        // Add a new item to the database
        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {
            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");
            
            try
            {
                _context.FileIndex.Add(updateStatusContent);
                _context.SaveChanges();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(updateStatusContent.FilePath);
                Console.WriteLine(e);
                throw;
            }

            return updateStatusContent;
        }
        
        // Remove a new item from the database
        public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
        {
            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");

            _context.FileIndex.Remove(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }

    }
}
