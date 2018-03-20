using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public partial class Query
    {
        // When input a direct file
        //        => if this file exist on the file system 
        //              => check if the hash in the db is up to date

        public void SyncSingleFile(string subPath = "")
        {
            if (Files.IsFolderOrFile(subPath) == FolderOrFileModel.FolderOrFileTypeList.File) // false == file
            {
                // single file -- update or adding
                var dbListWithOneFile = new List<FileIndexItem>();
                var dbItem = GetObjectByFilePath(subPath);
                if (dbItem != null)
                {
                    dbListWithOneFile.Add(dbItem);
                }

                var localListWithOneFileDbStyle = new List<string>();
                localListWithOneFileDbStyle.Add(subPath);

                CheckMd5Hash(localListWithOneFileDbStyle, dbListWithOneFile);
                AddPhotoToDatabase(localListWithOneFileDbStyle, dbListWithOneFile);

                throw new FileNotFoundException();
            }

        }
    }
}
