using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.Models;

namespace starsky.foundation.metathumbnail.Interfaces
{
	public interface IWriteMetaThumbnail
	{
		Task<bool> WriteAndCropFile(string fileHash,
			OffsetModel offsetData, int sourceWidth,
			int sourceHeight, FileIndexItem.Rotation rotation, 
			string reference = null);
	}
}
