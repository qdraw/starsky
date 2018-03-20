﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.Models;

namespace starsky.Services
{
    public partial class Query
    {
        // Add new photo to database 
        //  (if photo does not exist)

        public void AddPhotoToDatabase(
            List<string> localSubFolderDbStyle,
            List<FileIndexItem> databaseFileList
        )
        {
            foreach (var singleFolderDbStyle in localSubFolderDbStyle)
            {

                // Check if Photo is in database
                var dbFolderMatchFirst = databaseFileList.FirstOrDefault(
                    p =>
                        !p.IsDirectory &&
                        p.FilePath == singleFolderDbStyle
                );

                // Console.WriteLine(singleFolderDbStyle);

                if (dbFolderMatchFirst == null)
                {
                    // photo
                    Console.Write(".");
                    var singleFilePath = FileIndexItem.DatabasePathToFilePath(singleFolderDbStyle);
                    var databaseItem = ExifRead.ReadExifFromFile(singleFilePath);
                    databaseItem.AddToDatabase = DateTime.UtcNow;
                    databaseItem.FileHash = FileHash.CalcHashCode(singleFilePath);
                    databaseItem.FileName = Path.GetFileName(singleFilePath);
                    databaseItem.IsDirectory = false;
                    databaseItem.ParentDirectory = FileIndexItem.FullPathToDatabaseStyle(Path.GetDirectoryName(singleFilePath));
                    databaseItem.FilePath = singleFolderDbStyle;

                    AddItem(databaseItem);
                    databaseFileList.Add(databaseItem);
                }

                // end photo
            }
        }
    }
}
