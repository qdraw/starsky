using System;
using System.Threading.Tasks;

namespace starsky.foundation.writemeta.Interfaces
{
    public interface IExifTool
    {
	    Task<bool> WriteTagsAsync(string subPath, string command,
		    DateTime lastWriteTime);
	    Task<bool> WriteTagsThumbnailAsync(string fileHash, string command);
    }
}
