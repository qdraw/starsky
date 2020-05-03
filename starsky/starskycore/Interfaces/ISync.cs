using System.Collections.Generic;

namespace starskycore.Interfaces
{
    public interface ISync
    {
        IEnumerable<string> SyncFiles(string subPath, bool recursive = true);

	    /// <summary>
	    /// Adding parent folders
	    /// </summary>
	    /// <param name="subPath">relative urls</param>
	    void AddSubPathFolder(string subPath);

	    IEnumerable<string>
		    OrphanFolder(string subPath, int maxNumberOfItems = 3000);
    }
}
