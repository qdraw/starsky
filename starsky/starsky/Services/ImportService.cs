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
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;

        public ImportService(ApplicationDbContext context, ISync isync, IExiftool exiftool, AppSettings appSettings)
        {
            _context = context;
            _isync = isync;
            _exiftool = exiftool;
            _appSettings = appSettings;
        }


        // Imports a list of paths, used by the importer web interface
        public List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
        {
            var output = new List<string>();
            foreach (var inputFullPath in inputFullPathList)
            {
                output.Add(Import(inputFullPath, importSettings).FirstOrDefault());
            }

            // Remove duplicate and empty-strings-from-list
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            return output;
        }

        // Imports a single path, used by the cli importer
        public List<string> Import(string inputFullPath, ImportSettingsModel importSettings)
        {
            if (!Directory.Exists(inputFullPath) && File.Exists(inputFullPath))
            {
                // file
                var succesfullFullPaths = ImportFile(inputFullPath, importSettings);
                return new List<string> {succesfullFullPaths};
            }

            if (File.Exists(inputFullPath) || !Directory.Exists(inputFullPath))
            {
                Console.WriteLine("File already exist or not found " + inputFullPath);
                return new List<string>();
            }

            var filesFullPathList = new List<string>();
            // recursive
            if(importSettings.RecursiveDirectory) filesFullPathList = Files.GetFilesRecrusive(inputFullPath).ToList();
            // non-recursive
            if(!importSettings.RecursiveDirectory) filesFullPathList = Files.GetFilesInDirectory(inputFullPath,_appSettings).ToList();

            var succesfullDirFullPaths = Import(filesFullPathList, importSettings);
                
            return succesfullDirFullPaths;
        }

        // Create a new import object
        public ImportIndexItem ObjectCreateIndexItem(
            string inputFileFullPath, 
            string fileHashCode, 
            FileIndexItem fileIndexItem,
            string overwriteStructure)
        {
            var importIndexItem = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = inputFileFullPath,
                DateTime = fileIndexItem.DateTime,
                FileHash = fileHashCode
            };

            // Feature to overwrite structures when importing using a header
            // Overwrite the structure in the ImportIndexItem
            if (!string.IsNullOrWhiteSpace(overwriteStructure))
            {
                importIndexItem.Structure = overwriteStructure;
            }
            
            fileIndexItem.FileName = importIndexItem.ParseFileName();
            return importIndexItem;
        }

        public string DestionationFullPathDuplicate(
            string inputFileFullPath, 
            FileIndexItem fileIndexItem, 
            bool tryagain)
        {

            var destinationFullPath = _appSettings.DatabasePathToFilePath(fileIndexItem.ParentDirectory)
                                      + fileIndexItem.FileName;
            // When a file already exist, when you have multiple files with the same datetime
            if (inputFileFullPath != destinationFullPath
                && File.Exists(destinationFullPath) )
            {
               
                fileIndexItem.FileName = string.Concat(
                    Path.GetFileNameWithoutExtension(fileIndexItem.FileName),
                    DateTime.UtcNow.ToString("-fff"),
                    Path.GetExtension(fileIndexItem.FileName)
                );
                
                destinationFullPath = _appSettings.DatabasePathToFilePath(fileIndexItem.ParentDirectory)
                                      + fileIndexItem.FileName;
            }

            // For example when the number (ff) is already used:
            if (!tryagain || !File.Exists(destinationFullPath)) return destinationFullPath;
            destinationFullPath = DestionationFullPathDuplicate(inputFileFullPath,fileIndexItem,false);

            return File.Exists(destinationFullPath) ? null : destinationFullPath;
        }

        private string ImportFile(string inputFileFullPath, ImportSettingsModel importSettings)
        {
            var exifToolSync = false;
            
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
            // If is in the database, ignore it and don't delete it
            if (IsHashInImportDb(fileHashCode)) return string.Empty;

            // Only accept files with correct meta data
            var fileIndexItem = ExifRead.ReadExifFromFile(inputFileFullPath);

            // Check if there is a xmp file that contains data
            fileIndexItem = new XmpReadHelper(_appSettings).XmpSelectSidecarFile(fileIndexItem, inputFileFullPath);

            // Parse the filename and create a new importIndexItem object
            var importIndexItem = ObjectCreateIndexItem(inputFileFullPath, fileHashCode, fileIndexItem, importSettings.Structure);


            
            // Parse DateTime from filename
            if (fileIndexItem.DateTime < DateTime.UtcNow.AddYears(-2))
            {
                fileIndexItem.DateTime = importIndexItem.ParseDateTimeFromFileName();
                // > for the exif based items it is done in: ObjectCreateIndexItem()
                fileIndexItem.FileName = importIndexItem.ParseFileName();

                fileIndexItem.Description = "Date and Time based on filename";
                exifToolSync = true;
            }
            
            // Feature to ignore old files
            if (importSettings.AgeFileFilter && fileIndexItem.DateTime < DateTime.UtcNow.AddYears(-2))
            {
                if (_appSettings.Verbose) 
                    Console.WriteLine("use this structure to parse: " + _appSettings.Structure + "or " + importIndexItem.Structure);
                
                Console.WriteLine("> "+ inputFileFullPath 
                                      +  " is older than 2 years, "+
                                      "please use the -a flag to overwrite this; skip this file;");
                return string.Empty;
            }
            
            fileIndexItem.ParentDirectory = importIndexItem.ParseSubfolders();
            fileIndexItem.FileHash = fileHashCode;
            
            // Feature to overwrite default ColorClass Setting
            // First check and I is defferent than default enable sync
            fileIndexItem.SetColorClass(importSettings.ColorClass.ToString());
            if (fileIndexItem.ColorClass != FileIndexItem.Color.None)
                exifToolSync = true;


            
            var destinationFullPath = DestionationFullPathDuplicate(inputFileFullPath,fileIndexItem,true);
            
            if (destinationFullPath == null) Console.WriteLine("> "+ inputFileFullPath 
                                                                   + " "  + fileIndexItem.FileName 
                                                                   +  " Please try again > to many failures;");
            if (destinationFullPath == null) return string.Empty;
            
            File.Copy(inputFileFullPath, destinationFullPath);
            
            // Update the contents to the file the imported item
            if (exifToolSync)
            {
                var exiftoolmodel = new ExifToolModel
                {
                    AllDatesDateTime = fileIndexItem.DateTime,
                    CaptionAbstract = fileIndexItem.Description,
                    ColorClass = fileIndexItem.ColorClass
                };
                    
                _exiftool.Update(exiftoolmodel, destinationFullPath);
            }
            
            var syncFiles = _isync.SyncFiles(fileIndexItem.FilePath).ToList();
            // import has failed it has a list with one item with a emphy string
            if(syncFiles.FirstOrDefault() == string.Empty) return string.Empty;
            
            AddItem(importIndexItem);

            if (importSettings.DeleteAfter)
            {
                File.Delete(inputFileFullPath);
            }
            
            return fileIndexItem.FilePath;
        }
        
        
        // Add a new item to the database
        private void AddItem(ImportIndexItem updateStatusContent)
        {
//            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");
            updateStatusContent.AddToDatabase = DateTime.UtcNow;
            
            _context.ImportIndex.Add(updateStatusContent);
            _context.SaveChanges();
            // removed MySqlException catch
        }
        
        // Remove a new item from the database
        public ImportIndexItem RemoveItem(ImportIndexItem updateStatusContent)
        {
//            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");

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