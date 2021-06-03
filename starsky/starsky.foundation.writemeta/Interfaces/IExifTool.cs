using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.writemeta.Interfaces
{
    public interface IExifTool
    {
	    Task<KeyValuePair<bool, string>> WriteTagsAsync(string subPath, string command);
	    Task<bool> WriteTagsThumbnailAsync(string fileHash, string command);
    }
}
