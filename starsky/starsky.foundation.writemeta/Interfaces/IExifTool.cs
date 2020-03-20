using System.Threading.Tasks;

namespace starsky.foundation.writemeta.Interfaces
{
    public interface IExifTool
    {
	    Task<bool> WriteTagsAsync(string subPath, string command);
	    Task<bool> WriteTagsThumbnailAsync(string fileHash, string command);
    }
}
