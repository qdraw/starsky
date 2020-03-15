using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starskycore.Models;

namespace starskycore.Helpers
{
	
	/// <summary>
	/// WARNING; class is obsolete
	/// </summary>
	[Obsolete("Will be removed in the 0.2.1 release")] 
    public static class FilesHelper
    {
	    
	    [Obsolete("Will be removed in the 0.2.1 release")] 
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [Obsolete("Will be removed in the 0.2.1 release")] 
        public static void DeleteFile(IEnumerable<string> toDeletePaths)
        {
            foreach (var toDelPath in toDeletePaths)
            {
                if (File.Exists(toDelPath))
                {
                    File.Delete(toDelPath);
                }
            }
        }

    }
}
