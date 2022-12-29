using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metathumbnail.Models;

namespace starsky.foundation.metathumbnail.Interfaces
{
	public interface IWriteMetaThumbnailService
	{
		Task<bool> WriteAndCropFile(string fileHash,
			OffsetModel offsetData, int sourceWidth,
			int sourceHeight, FileIndexItem.Rotation rotation, 
			string? reference = null);
	}
}
