using System;
using System.IO;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // When input a direct file
        //        => if this file is deleted on the file system 
        //              => delete it from the database
        
        // True is stop after
        // False is continue
        
        private bool Deleted(string subPath = "")
        {
            subPath = _query.SubPathSlashRemove(subPath);

            Console.WriteLine(Files.IsFolderOrFile(subPath));
            
            if (Files.IsFolderOrFile(subPath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
            {
                Console.WriteLine(">>deleted");
                // single file or folder deleting
                var dbItem = _query.GetObjectByFilePath(subPath);
                if (dbItem != null)
                {
                    
                    _query.RemoveItem(dbItem);
                    Console.WriteLine("File " + subPath +" not found and removed");

                    if (dbItem.IsDirectory == false) return true;
                    // Remove subitems in directory
                    var toBeDeleted = _query.GetAllFiles(dbItem.FilePath);

                    foreach (var item in toBeDeleted)
                    {
                        Console.WriteLine("|");
                        _query.RemoveItem(item);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
