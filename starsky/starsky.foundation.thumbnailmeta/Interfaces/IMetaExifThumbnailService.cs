using System.Threading.Tasks;

namespace starsky.foundation.metathumbnail.Interfaces
{
	public interface IMetaExifThumbnailService
	{
		Task<bool> AddMetaThumbnail(string subPath);
		Task<bool> AddMetaThumbnail(string subPath, string fileHash);
	}
}
