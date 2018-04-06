using System;
using System.Collections.Generic;
using System.Linq;

namespace starsky.Services
{
    public static class Breadcrumbs
    {
        // Breadcrumb returns a list of parent folders
        // it does not contain the current folder
        
        public static List<string> BreadcrumbHelper(string filePath)
        {
            if (filePath == null) return new List<string>(); 

            // remove backslash from end
            filePath = ConfigRead.RemoveLatestBackslash(filePath);

            var breadcrumb = new List<string>();
            if (filePath[0].ToString() != "/")
            {
                filePath = "/" + filePath;
            }
            var filePathArray = filePath.Split("/");

            var dir = 0;
            while (dir < filePathArray.Length - 1)
            {
                if (String.IsNullOrEmpty(filePathArray[dir]))
                {
                    breadcrumb.Add("/");
                }
                else
                {
                    var item = "";
                    for (int i = 0; i <= dir; i++)
                    {
                        if (!String.IsNullOrEmpty(filePathArray[i]))
                        {
                            item += "/" + filePathArray[i];
                        }
                    }
                    breadcrumb.Add(item);
                }
                dir++;
            }
            return breadcrumb;
        }
    }
}
