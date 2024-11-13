using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.Models;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;

public interface IWritePreviewThumbnailService
{
	Task<bool> WriteFile(string fileHash,
		OffsetModel offsetData, RotationModel.Rotation rotation,
		string? reference = null);
}
