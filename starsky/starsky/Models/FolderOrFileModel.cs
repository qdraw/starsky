using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.Models
{
    public class FolderOrFileModel
    {
        public FolderOrFileTypeList IsFolderOrFile { get; set; }
        public enum FolderOrFileTypeList
        {
            Folder = 1,
            File = 2,
            Deleted = 0
        }
    }
}
