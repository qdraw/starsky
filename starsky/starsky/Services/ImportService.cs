using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public class ImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISync _isync;

        public ImportService(ApplicationDbContext context, ISync isync)
        {
            _context = context;
            _isync = isync;
        }

        public List<string> Import(string inputFullPath, bool deleteAfter = false)
        {
            if (!Directory.Exists(inputFullPath) && File.Exists(inputFullPath))
            {
                // file
                var succesfullFullPaths = ImportFile(inputFullPath, deleteAfter);
                return new List<string> {succesfullFullPaths};
            }

            if (File.Exists(inputFullPath) || !Directory.Exists(inputFullPath)) return new List<string>();

            // Directory
            var succesfullDirFullPaths = new List<string>();
            var filesFullPath = Files.GetFilesInDirectory(inputFullPath,false);
            foreach (var item in filesFullPath)
            {
                // Directory
                var fullPath = ImportFile(item, deleteAfter);
                succesfullDirFullPaths.Add(fullPath);
            }
            return succesfullDirFullPaths;
        }

        public ImportIndexItem SetupImportIndexItem(string inputFileFullPath, string fileHashCode, FileIndexItem fileIndexItem)
        {
            var importIndexItem = new ImportIndexItem
            {
                SourceFullFilePath = inputFileFullPath,
                DateTime = fileIndexItem.DateTime,
                FileHash = fileHashCode
            };

            fileIndexItem.FileName = importIndexItem.ParseFileName();
            return importIndexItem;
        }

        private string ImportFile(string inputFileFullPath, bool deleteAfter = false)
        {
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
            // If is in the database, ignore it and dont delete it
            if (IsHashInDatabase(fileHashCode)) return string.Empty;

            // Only exepts files with correct meta data
            var fileIndexItem = ExifRead.ReadExifFromFile(inputFileFullPath);

            var importIndexItem = SetupImportIndexItem(inputFileFullPath, fileHashCode, fileIndexItem);
            
            var fullDestFolderPath = FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders()) ;
            
            fileIndexItem.ParentDirectory = FileIndexItem.FullPathToDatabaseStyle(fullDestFolderPath);
            fileIndexItem.FilePath = FileIndexItem.FullPathToDatabaseStyle(fullDestFolderPath) +
                                     Path.DirectorySeparatorChar + fileIndexItem.FileName;
            fileIndexItem.FileHash = fileHashCode;

            var destinationFullPath = Path.Combine(fullDestFolderPath, fileIndexItem.FileName);

            if (inputFileFullPath != destinationFullPath 
                && !File.Exists(destinationFullPath))
            {
                File.Copy(inputFileFullPath, destinationFullPath);
            }

            _isync.SyncFiles(fileIndexItem.FilePath);
            
            AddItem(importIndexItem);

            if (deleteAfter)
            {
                File.Delete(inputFileFullPath);
            }
            
            return inputFileFullPath;
        }
        
        
        // Add a new item to the database
        private ImportIndexItem AddItem(ImportIndexItem updateStatusContent)
        {
            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");
            
            try
            {
                _context.ImportIndex.Add(updateStatusContent);
                _context.SaveChanges();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(updateStatusContent);
                Console.WriteLine(e);
                throw;
            }

            return updateStatusContent;
        }
        
       
        public bool IsHashInDatabase(string fileHash)
        {
            var query = _context.ImportIndex.Any(
                p => p.FileHash == fileHash 
            );
            return query;
        }
    }
    
    
}