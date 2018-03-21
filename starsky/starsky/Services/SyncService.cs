using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService : ISync
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuery _query;

        public SyncService(ApplicationDbContext context, IQuery query)
        {
            _context = context;
            _query = query;

        }
        public IEnumerable<string> SyncFiles(string subPath = "")
        {
            Deleted(subPath);
            SingleFile(subPath);

            // if folder: 
            var localSubFolderDbStyle = _renameListItemsToDbStyle(
                Files.GetAllFilesDirectory(subPath).ToList()
            );


            var databaseSubFolderList = _context.FileIndex.Where(p => p.IsDirectory).ToList();

            // Sync for folders
            RemoveOldFilePathItemsFromDatabase(localSubFolderDbStyle, databaseSubFolderList, subPath);
            AddFoldersToDatabase(localSubFolderDbStyle, databaseSubFolderList);

            Console.WriteLine(".");

            // Allow sync for direct folder
            localSubFolderDbStyle.Add(subPath);

            foreach (var singleFolder in localSubFolderDbStyle)
            {
                Console.Write(singleFolder + "  ");

                var databaseFileList = _query.GetAllFiles(singleFolder);
                Console.WriteLine(databaseFileList);
                var localFarrayFilesDbStyle = Files.GetFilesInDirectory(singleFolder).ToList();

                databaseFileList = RemoveOldFilePathItemsFromDatabase(localFarrayFilesDbStyle, databaseFileList, subPath);
                CheckMd5Hash(localFarrayFilesDbStyle, databaseFileList);
                AddPhotoToDatabase(localFarrayFilesDbStyle, databaseFileList);
                Console.WriteLine("-");
            }

            AddSubPathFolder(subPath);

            return null;
        }

        private List<string> _renameListItemsToDbStyle(List<string> localSubFolderList)
        {
            var localSubFolderListDatabaseStyle = new List<string>();

            foreach (var item in localSubFolderList)
            {
                localSubFolderListDatabaseStyle.Add(FileIndexItem.FullPathToDatabaseStyle(item));
            }

            return localSubFolderListDatabaseStyle;
        }

    }
}
