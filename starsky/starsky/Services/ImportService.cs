using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using starsky.Data;
using starsky.Models;

namespace starsky.Services
{
    public class ImportService
    {
        private readonly ApplicationDbContext _context;

        public ImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<string> ImportFile(string inputFileFullPath)
        {
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
            // If is in the database
            if(IsHashInDatabase(fileHashCode)) return new List<string>();

            // Only exepts files with correct meta data
            var model = ExifRead.ReadExifFromFile(inputFileFullPath);
            
            // You need to update this after moving file
            model.FilePath = inputFileFullPath;
            model.FileHash = fileHashCode;

            var fileName = model.ParseFileName();
            var subFolders = model.ParseSubFolders();

            _checkIfSubDirectoriesExist(subFolders);

            return null;
        }

        private IEnumerable<string> _checkIfSubDirectoriesExist(List<string> folderStructure)
        {
            // Check if Dir exist

            if (folderStructure.FirstOrDefault() == "/") return null;

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
                Console.WriteLine("q> " + fullPathBase);
            }
              return new List<string>();
        }

        private IEnumerable<string> _checkIfSingleSubDirectorieExist(string singleSubFolder)
        {
            return new List<string>();

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