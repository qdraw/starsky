using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starskycore.Data;
using starskycore.Extensions;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
    public class ImportService : IImport
    {
        private ApplicationDbContext _context;
        private readonly ISync _isync;
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;
        private readonly IReadMeta _readmeta;
        private readonly IServiceScopeFactory _scopeFactory;
	    private readonly bool _isConnection;

        public ImportService(ApplicationDbContext context, 
            ISync isync, 
            IExiftool exiftool, 
            AppSettings appSettings, 
            IReadMeta readMeta, 
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
	        _isConnection = _context.TestConnection(appSettings);
		        
            _isync = isync;
            _exiftool = exiftool;
            _appSettings = appSettings;
            _readmeta = readMeta;
            _scopeFactory = scopeFactory;
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
                var successfulFullPaths = ImportFile(inputFullPath, importSettings);
                return new List<string> {successfulFullPaths};
            }

            if (File.Exists(inputFullPath) || !Directory.Exists(inputFullPath))
            {
                Console.WriteLine("File already exist or not found " + inputFullPath);
                return new List<string>();
            }

            var filesFullPathList = new List<string>();
            // recursive
            if(importSettings.RecursiveDirectory) filesFullPathList = FilesHelper.GetFilesRecrusive(inputFullPath).ToList();
            // non-recursive
            if(!importSettings.RecursiveDirectory) filesFullPathList = FilesHelper.GetFilesInDirectory(inputFullPath).ToList();

            var successfulDirFullPaths = Import(filesFullPathList, importSettings);
	        
	        if ( _appSettings.Verbose ) Console.WriteLine("Number of files = " + successfulDirFullPaths.Count);
            
	        return successfulDirFullPaths;
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

        /// <summary>
        /// Checks if file exist in storagefolder - or suggest a `-102` or `-909` appendex
        /// </summary>
        /// <param name="inputFileFullPath">the source file</param>
        /// <param name="fileIndexItem">the object with the data</param>
        /// <param name="tryagain">to avoid an endless loop</param>
        /// <returns>string with DestionationFullPath</returns>
        public string DestionationFullPathDuplicate(
            string inputFileFullPath, 
            FileIndexItem fileIndexItem, 
            bool tryagain)
        {

            // To support direct imported files > without filename
            if (fileIndexItem.FileName.Contains(".unknown"))
            {
                fileIndexItem.FileName = fileIndexItem.FileName.Replace(".unknown", 
                    "." + ExtensionRolesHelper.GetImageFormat(inputFileFullPath));
            }
            
            var destinationFullPath = _appSettings.DatabasePathToFilePath(fileIndexItem.ParentDirectory)
                                      + fileIndexItem.FileName;
            
            // When a file already exist, when you have multiple files with the same datetime
            if (inputFileFullPath != destinationFullPath
                && File.Exists(destinationFullPath))
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

        public FileIndexItem ReadExifAndXmpFromFile(string inputFileFullPath)
        {
            return _readmeta.ReadExifAndXmpFromFile(inputFileFullPath,ExtensionRolesHelper.GetImageFormat(inputFileFullPath));
        }

        public bool IsAgeFileFilter(ImportSettingsModel importSettings, DateTime exifDateTime)
        {
            return !importSettings.AgeFileFilterDisabled && exifDateTime < DateTime.UtcNow.AddYears(-2);
        }

        private string ImportFile(string inputFileFullPath, ImportSettingsModel importSettings)
        {
            var exifToolSync = false;
            
            var fileHashCode = FileHash.GetHashCode(inputFileFullPath);
            
	        // Check controller as well for the exist check:
	        
            // If is in the ImportIndex, ignore it and don't delete it
            if (importSettings.IndexMode && IsHashInImportDb(fileHashCode)) return string.Empty;
	        

            // Only accept files with correct meta data
            // Check if there is a xmp file that contains data
            var fileIndexItem = ReadExifAndXmpFromFile(inputFileFullPath);

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
            if (IsAgeFileFilter(importSettings, fileIndexItem.DateTime))
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
            fileIndexItem.ColorClass = fileIndexItem.GetColorClass(importSettings.ColorClass.ToString());
            if (fileIndexItem.ColorClass != FileIndexItem.Color.None)
                exifToolSync = true;


            
            var destinationFullPath = DestionationFullPathDuplicate(inputFileFullPath,fileIndexItem,true);
            
            if (destinationFullPath == null) Console.WriteLine("> "+ inputFileFullPath 
                                                                   + " "  + fileIndexItem.FileName 
                                                                   +  " Please try again > to many failures;");
            if (destinationFullPath == null) return string.Empty;
            
            File.Copy(inputFileFullPath, destinationFullPath);
            
            // Update the contents to the file the imported item
            if (exifToolSync && ExtensionRolesHelper.IsExtensionExifToolSupported(inputFileFullPath))
            {
                Console.WriteLine("Do a exiftoolSync");
                var comparedNamesList = new List<string>
                {
                    nameof(FileIndexItem.DateTime),
                    nameof(FileIndexItem.ColorClass),
                    nameof(FileIndexItem.Description),
                };

                new ExifToolCmdHelper(_appSettings, _exiftool).Update(fileIndexItem, destinationFullPath,
                    comparedNamesList);
            }
            
	        // Ignore the sync part if the connection is missing
	        // or option enabled
	        if ( importSettings.IndexMode && _isConnection )
	        {
	        
		        // The files that are imported need to be synced
		        var syncFiles = _isync.SyncFiles(fileIndexItem.FilePath).ToList();

		        // import has failed it has a list with one item with a empty string
		        if ( syncFiles.FirstOrDefault() == string.Empty) return string.Empty;
            
		        // To the list of imported folders
		        AddItem(importIndexItem);
		        
	        }

	        if ( _appSettings.Verbose ) Console.Write(".");
	        
	        
			// setting	        
            if (importSettings.DeleteAfter)
            {
                File.Delete(inputFileFullPath);
            }

	        return fileIndexItem.FilePath;
        }
        
        
        // Add a new item to the database
        private void AddItem(ImportIndexItem updateStatusContent)
        {
            updateStatusContent.AddToDatabase = DateTime.UtcNow;
            
            _context.ImportIndex.Add(updateStatusContent);
            _context.SaveChanges();
            // removed MySqlException catch
        }
        
        // Remove a new item from the database
        public ImportIndexItem RemoveItem(ImportIndexItem updateStatusContent)
        {
            _context.ImportIndex.Remove(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }
        
        // Return a File Item By it Hash value
        // New added, directory hash now also hashes
        public ImportIndexItem GetItemByHash(string fileHash)
        {
            InjectServiceScope();
            var query = _context.ImportIndex.FirstOrDefault(
                p => p.FileHash == fileHash 
            );
            return query;
        }

//	    /// <summary>
//	    /// Check for _query and ImportIndex table
//	    /// </summary>
//	    /// <param name="fileHashCode"></param>
//	    /// <returns>True = contains any value</returns>
//	    public bool IsHashInImportAndQueryDb(string fileHashCode)
//	    {
//		    InjectServiceScope();
//
//		    // If in FileHash Table
//		    if ( _isConnection && 
//		         _context.FileIndex.Any(p => p.FileHash == fileHashCode 
//		                                     && p.Tags.Contains("!delete!") == false 
//		                                     && p.IsDirectory == false)) return true;
//
//		    return IsHashInImportDb(fileHashCode);
//	    }

	    public bool IsHashInImportDb(string fileHashCode)
        {
			InjectServiceScope();

			if ( _isConnection )
				return _context.ImportIndex.Any(
					p => p.FileHash == fileHashCode
				);
	        
			// When there is no mysql connection continue
			Console.WriteLine($">> _isConnection == false -- fileHash:{fileHashCode}");
			return false;

        }
	    
	    /// <summary>
	    /// Query Database for all items
	    /// </summary>
	    /// <returns></returns>
	    public List<ImportIndexItem> GetAll()
	    {
		    return _context.ImportIndex.Where(p => p.FileHash != null).ToList();
	    }

        /// <summary>
        /// Dependency injection, used in background tasks
        /// </summary>
        private void InjectServiceScope()
        {
            if (_scopeFactory == null) return;
            var scope = _scopeFactory.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }
        
    }
    
    
}
