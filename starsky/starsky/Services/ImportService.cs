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

        public List<string> Import(string inputFullPath)
        {
            if (!Directory.Exists(inputFullPath) && File.Exists(inputFullPath))
            {
                // file
                var succesfullFullPaths = ImportFile(inputFullPath);
                return new List<string> {succesfullFullPaths};
            }

            if (File.Exists(inputFullPath) || !Directory.Exists(inputFullPath)) return new List<string>();

            // Directory
            var succesfullDirFullPaths = new List<string>();
            var filesFullPath = Files.GetFilesInDirectory(inputFullPath,false);
            foreach (var item in filesFullPath)
            {
                // Directory
                var fullPath = ImportFile(item);
                succesfullDirFullPaths.Add(fullPath);
            }
            return succesfullDirFullPaths;
        }


        private string ImportFile(string inputFileFullPath)
        {
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
            // If is in the database, ignore it and dont delete it
            if (IsHashInDatabase(fileHashCode)) return string.Empty;

            // Only exepts files with correct meta data
            var fileIndexItem = ExifRead.ReadExifFromFile(inputFileFullPath);
            var importIndexItem = new ImportIndexItem
            {
                SourceFullFilePath = inputFileFullPath,
                DateTime = fileIndexItem.DateTime
            };
            importIndexItem.ParseFileName();

//            
//            // You need to update this after moving file
//            model.FilePath = inputFileFullPath;
//            model.FileHash = fileHashCode;
//
//            model.FileName = model.ParseFileName();
//            var subFolders = model.ParseSubFolders();
//
//            var destFolder = _checkIfSubDirectoriesExist(subFolders);
//
//            var destinationFullPath = Path.Combine(destFolder, model.FileName);
//
//            if (inputFileFullPath != destinationFullPath 
//                && !File.Exists(destinationFullPath))
//            {
//                File.Copy(inputFileFullPath, destinationFullPath);
//            }
//
//            var destinationSubPath = FileIndexItem.FullPathToDatabaseStyle(destinationFullPath);
//            _isync.SyncFiles(destinationSubPath);
//
//            // Add item to Import database
//            var indexItem = new ImportIndexItem
//            {
//                AddToDatabase = DateTime.UtcNow,
//                FileHash = fileHashCode
//            };
//            AddItem(indexItem);
//            return inputFileFullPath;
            return null;
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
                fullPathBase += folder.Replace("*", string.Empty) + Path.DirectorySeparatorChar;
                var isDeleted = !Directory.Exists(fullPathBase);
                if (isDeleted)
                {
                    Directory.CreateDirectory(fullPathBase);
                }
            }

            return fullPathBase;
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