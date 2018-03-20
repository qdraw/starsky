using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.Models;

namespace starsky.Services
{
    public partial class Query
    {
        // AddSubPathFolder
        //  root(/)
        //      /2017  <= index only this folder
        //      /2018
        // If you use the cmd: $ starskycli -s "/2017"
        // the folder '2017' it self is not added 
        // and all parrent paths are not included
        // this class does add those parent folders

        public void AddSubPathFolder(string subPath)
        {
            subPath = SubPathSlashRemove(subPath);

            var listOfSubpaths = Breadcrumbs.BreadcrumbHelper(subPath);
            listOfSubpaths.Add(subPath);

            foreach (var itemSubpath in listOfSubpaths)
            {
                var countFolder = _context.FileIndex.Count(p => p.FilePath == itemSubpath);
                if (countFolder == 0)
                {
                    var newItem = new FileIndexItem();
                    newItem.FilePath = itemSubpath;
                    newItem.IsDirectory = true;
                    if (itemSubpath != "/")
                    {
                        newItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(itemSubpath).LastOrDefault();
                    }
                    newItem.FileName = itemSubpath.Split("/").LastOrDefault();
                    AddItem(newItem);
                }
            }
        }
    }
}
