using System.Threading.Tasks;

namespace starskycore.Interfaces
{
    public interface IExifTool
    {
	    Task<bool> WriteTagsAsync(string subPath, string command);
	    Task<bool> WriteTagsThumbnailAsync(string fileHash, string command);
    }
}
