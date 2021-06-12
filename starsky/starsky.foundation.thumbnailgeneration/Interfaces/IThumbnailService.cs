using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailService
	{
		Task<bool> CreateThumb(string subPath);
		Task<bool> CreateThumb(string subPath, string fileHash);
	}
}
