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
            
            // Reading patterns from .config file
            var fileNamePatterns = _getFilenamePattern();
            var foldersPatterns = _getFoldersPattern();

            // Parse datetime values
            var fileNameStructureList = _parseListDateFormat(fileNamePatterns,model.DateTime);
            var folderStructure = _parseListDateFormat(foldersPatterns,model.DateTime);

            // Do the extension fix
            var fileNameStructure = _getFileNameFromDatePatern(
                fileNameStructureList, inputFileFullPath);

            _checkIfSubDirectoriesExist(folderStructure);

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


        private string _getFileNameFromDatePatern(
            IEnumerable<string> fileNameStructureList, 
            string inputFileFullPath)
        {
            var fileExtenstion = Files.GetImageFormat(inputFileFullPath).ToString();
            
            foreach (var item in fileNameStructureList)
            {
                Regex rgx = new Regex(".ex[A-Z]$");
                var result = rgx.Replace(item, "." + fileExtenstion);
                return result;
            }
            return string.Empty;
        }

        private List<string> _getFilenamePattern()
        {
            // 20180419_164921.exP
            var structureAllSplit = AppSettingsProvider.Structure.Split("/");
            if (structureAllSplit.Length <= 2) Console.WriteLine("Should be protected by Model");

            return new List<string> {structureAllSplit[structureAllSplit.Length - 1]};
        }

        
        private static List<string> _getFoldersPattern()
        {
            var structureAllSplit = AppSettingsProvider.Structure.Split("/");

            if (structureAllSplit.Length <= 2)
            {
                var list = new List<string> {"/"};
                return list;
            }
            // Return if nothing only a list with one slash


            var structure = new List<string>();
            for (int i = 1; i < structureAllSplit.Length-1; i++)
            {
                structure.Add(structureAllSplit[i]);
            }
            // else return the subfolders
            return structure;
        }

        private static List<string> _parseListDateFormat(List<string> patternList, DateTime fileDateTime)
        {
            var parseListDate = new List<string>();
            foreach (var patternItem in patternList)
            {
                if (patternItem == "/") return patternList;
                var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
                parseListDate.Add(item);
            }
            return parseListDate;
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