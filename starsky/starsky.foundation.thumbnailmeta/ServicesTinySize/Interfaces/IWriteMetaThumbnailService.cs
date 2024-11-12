using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.Models;

namespace starsky.foundation.thumbnailmeta.ServicesTinySize.Interfaces;

public interface IWriteMetaThumbnailService
{
	Task<bool> WriteAndCropFile(string fileHash,
		OffsetModel offsetData, int sourceWidth,
		int sourceHeight, RotationModel.Rotation rotation,
		string? reference = null);
}
