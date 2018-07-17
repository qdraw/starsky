using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public class ImportService : IImport
    {
        private readonly ApplicationDbContext _context;
        private readonly ISync _isync;

        public ImportService(ApplicationDbContext context, ISync isync)
        {
            _context = context;
            _isync = isync;
        }

        // Imports a list of paths, used by the importer web interface
        public List<string> Import(IEnumerable<string> inputFullPathList, bool deleteAfter = false, bool ageFileFilter = true)
        {
            var output = new List<string>();
            foreach (var inputFullPath in inputFullPathList)
            {
                output.Add(Import(inputFullPath, deleteAfter, ageFileFilter).FirstOrDefault());
            }
            return output;
        }

        // Imports a single path, used by the cli importer
        public List<string> Import(string inputFullPath, bool deleteAfter = false, bool ageFileFilter = true)
        {
            if (!Directory.Exists(inputFullPath) && File.Exists(inputFullPath))
            {
                // file
                var succesfullFullPaths = ImportFile(inputFullPath, deleteAfter, ageFileFilter);
                return new List<string> {succesfullFullPaths};
            }

            if (File.Exists(inputFullPath) || !Directory.Exists(inputFullPath))
            {
                Console.WriteLine("File already exist or not found " + inputFullPath);
                return new List<string>();
            }

            // Directory
            var succesfullDirFullPaths = new List<string>();
            var filesFullPath = Files.GetFilesInDirectory(inputFullPath,false);
            foreach (var item in filesFullPath)
            {
                // Directory
                var fullPath = ImportFile(item, deleteAfter, ageFileFilter);
                succesfullDirFullPaths.Add(fullPath);
            }
            return succesfullDirFullPaths;
        }

        public ImportIndexItem ObjectCreateIndexItem(string inputFileFullPath, string fileHashCode, FileIndexItem fileIndexItem)
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

        public string DestionationFullPathDuplicate(string inputFileFullPath, FileIndexItem fileIndexItem, bool tryagain)
        {

            var destinationFullPath = FileIndexItem.DatabasePathToFilePath(fileIndexItem.ParentDirectory)
                                      + fileIndexItem.FileName;
            // When a file already exist, when you have multiple files with the same datetime
            if (inputFileFullPath != destinationFullPath
                && File.Exists(destinationFullPath) )
            {
               
                fileIndexItem.FileName = string.Concat(
                    Path.GetFileNameWithoutExtension(fileIndexItem.FileName),
                    DateTime.UtcNow.ToString("-ff"),
                    Path.GetExtension(fileIndexItem.FileName)
                );
                
                destinationFullPath = FileIndexItem.DatabasePathToFilePath(fileIndexItem.ParentDirectory)
                                      + fileIndexItem.FileName;
            }

            // For example when the number (ff) is already used:
            if (!tryagain || !File.Exists(destinationFullPath)) return destinationFullPath;
            destinationFullPath = DestionationFullPathDuplicate(inputFileFullPath,fileIndexItem,false);

            return File.Exists(destinationFullPath) ? null : destinationFullPath;
        }

        private string ImportFile(string inputFileFullPath, bool deleteAfter = false, bool ageFileFilter = true)
        {
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
            // If is in the database, ignore it and dont delete it
            if (IsHashInImportDb(fileHashCode)) return string.Empty;

            // Only accept files with correct meta data
            var fileIndexItem = ExifRead.ReadExifFromFile(inputFileFullPath);
            
            if (ageFileFilter && fileIndexItem.DateTime < DateTime.UtcNow.AddYears(-2))
            {
                Console.WriteLine("> "+ inputFileFullPath +  " is older than 2 years, please use the -a flag to overwrite this; skip this file;");
                return string.Empty;
            }
            
            var importIndexItem = ObjectCreateIndexItem(inputFileFullPath, fileHashCode, fileIndexItem);

            fileIndexItem.ParentDirectory = importIndexItem.ParseSubfolders();
            fileIndexItem.FileHash = fileHashCode;

            var destinationFullPath = DestionationFullPathDuplicate(inputFileFullPath,fileIndexItem,true);
            
            if (destinationFullPath == null) Console.WriteLine("> "+ inputFileFullPath + " "  + fileIndexItem.FileName +  " Please try again > to many failures;");
            if (destinationFullPath == null) return string.Empty;
            
            File.Copy(inputFileFullPath, destinationFullPath);

            _isync.SyncFiles(fileIndexItem.FilePath);
            
            AddItem(importIndexItem);

            if (deleteAfter)
            {
                File.Delete(inputFileFullPath);
            }
            
            return inputFileFullPath;
        }
        
        
        // Add a new item to the database
        private void AddItem(ImportIndexItem updateStatusContent)
        {
            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");
            updateStatusContent.AddToDatabase = DateTime.UtcNow;
            
            _context.ImportIndex.Add(updateStatusContent);
            _context.SaveChanges();
            // removed MySqlException catch
        }
        
        // Remove a new item from the database
        public ImportIndexItem RemoveItem(ImportIndexItem updateStatusContent)
        {
            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");

            _context.ImportIndex.Remove(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }
        
        // Return a File Item By it Hash value
        // New added, directory hash now also hashes
        public ImportIndexItem GetItemByHash(string fileHash)
        {
            var query = _context.ImportIndex.FirstOrDefault(
                p => p.FileHash == fileHash 
            );
            return query;
        }
        
       
        public bool IsHashInImportDb(string fileHash)
        {
            var query = _context.ImportIndex.Any(
                p => p.FileHash == fileHash 
            );
            return query;
        }
    }
    
    
}