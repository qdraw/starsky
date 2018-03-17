using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.Services
{
    public class Breadcrumbs
    {
        public static List<string> BreadcrumbHelper(string filePath)
        {
            if (filePath == null) return null;

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
                        //else
                        //{
                        //    item += "/" +filePathArray[i];
                        //}
                    }
                    breadcrumb.Add(item);
                }
                dir++;

            }

            return breadcrumb;
        }
    }
}
