using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesTinySize.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIWriteMetaThumbnailService : IWriteMetaThumbnailService
{
	public Task<bool> WriteAndCropFile(string fileHash, OffsetModel offsetData, int sourceWidth,
		int sourceHeight, RotationModel.Rotation rotation, string? reference = null)
	{
		return Task.FromResult(true);
	}
}
