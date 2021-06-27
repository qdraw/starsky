using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.readmeta.Models;

namespace starskytest.FakeMocks
{
	public class FakeIWriteMetaThumbnailService : IWriteMetaThumbnailService
	{
		public Task<bool> WriteAndCropFile(string fileHash, OffsetModel offsetData, int sourceWidth,
			int sourceHeight, FileIndexItem.Rotation rotation, string reference = null)
		{
			return Task.FromResult(true);
		}
	}
}
