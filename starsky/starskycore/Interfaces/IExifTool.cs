using System.IO;
using System.Threading.Tasks;
using starskycore.Models;

namespace starskycore.Interfaces
{
    public interface IExifTool
    {
	    Task<bool> WriteTagsAsync(string subPath, string command);
	    Task<bool> WriteTagsThumbnailAsync(string fileHash, string command);
	    
//        string BaseCommmand(string options, string fullFilePathSpaceSeperated);
//        ExifToolModel Info(string fullFilePath);
    }
}