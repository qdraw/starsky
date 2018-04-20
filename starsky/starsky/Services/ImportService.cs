using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using starsky.Data;
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

        public void Import(string inputFullPath)
        {
            if (!Directory.Exists(inputFullPath) && File.Exists(inputFullPath))
            {
                // file
                ImportFile(inputFullPath);
            }

            if (!File.Exists(inputFullPath) && Directory.Exists(inputFullPath))
            {
                // Directory
                var filesFullPath = Files.GetFilesInDirectory(inputFullPath,false);
                foreach (var item in filesFullPath)
                {
                    ImportFile(item);
                }
            }

        }


        public void ImportFile(string inputFileFullPath)
        {
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
            // If is in the database
            if (IsHashInDatabase(fileHashCode)) return;

            // Only exepts files with correct meta data
            var model = ExifRead.ReadExifFromFile(inputFileFullPath);
            
            // You need to update this after moving file
            model.FilePath = inputFileFullPath;
            model.FileHash = fileHashCode;

            model.FileName = model.ParseFileName();
            var subFolders = model.ParseSubFolders();

            var destFolder = _checkIfSubDirectoriesExist(subFolders);

            var destinationFullPath = Path.Combine(destFolder, model.FileName);

            if (inputFileFullPath != destinationFullPath 
                && !File.Exists(destinationFullPath))
            {
                File.Copy(inputFileFullPath, destinationFullPath);
            }

            var destinationSubPath = FileIndexItem.FullPathToDatabaseStyle(destinationFullPath);
            _isync.SyncFiles(destinationSubPath);

            // Add item to Import database
            var indexItem = new ImportIndexItem
            {
                AddToDatabase = DateTime.UtcNow,
                FileHash = fileHashCode
            };
            AddItem(indexItem);
            
        }

        private string _checkIfSubDirectoriesExist(List<string> folderStructure)
        {
            // Return fullDirectory Path
            // Check if Dir exist

            if (folderStructure.FirstOrDefault() == "/") return string.Empty;

            folderStructure.Insert(0, string.Empty);

            var fullPathBase = FileIndexItem.DatabasePathToFilePath(folderStructure.FirstOrDefault());
                
            foreach (var folder in folderStructure)
            {
                fullPathBase += folder + Path.DirectorySeparatorChar;
                var isDeleted = !Directory.Exists(fullPathBase);
                if (isDeleted)
                {
                    Directory.CreateDirectory(fullPathBase);
                }
            }

            return fullPathBase;
        }


        
        
        // Add a new item to the database
        public ImportIndexItem AddItem(ImportIndexItem updateStatusContent)
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