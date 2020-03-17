using System.Collections.Generic;
using System.Text;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Helpers
{
    public static class Breadcrumbs
    {
	    
	    
	    /// <summary>
        /// Breadcrumb returns a list of parent folders
        /// it does not contain the current folder
        /// </summary>
        /// <param name="filePath">subpath (unix style)</param>
        /// <returns>list of parent folders</returns>
        public static List<string> BreadcrumbHelper(string filePath)
        {
            if (filePath == null) return new List<string>(); 

            // remove backslash from end
            filePath = PathHelper.RemoveLatestBackslash(filePath);

            var breadcrumb = new List<string>();
            if (filePath[0].ToString() != "/")
            {
                filePath = "/" + filePath;
            }
            var filePathArray = filePath.Split("/".ToCharArray());

            var dir = 0;
            while (dir < filePathArray.Length - 1)
            {
                if (string.IsNullOrEmpty(filePathArray[dir]))
                {
                    breadcrumb.Add("/");
                }
                else
                {
                    var itemStringBuilder = new StringBuilder();
                    
                    for (int i = 0; i <= dir; i++)
                    {
                        if (!string.IsNullOrEmpty(filePathArray[i]))
                        {
                            itemStringBuilder.Append("/" + filePathArray[i]);
                        }
                    }
                    breadcrumb.Add(itemStringBuilder.ToString());
                }
                dir++;
            }
            return breadcrumb;
        }
    }
}
