using System.Threading.Tasks;
using starsky.foundation.storage.Models;

namespace starskytest.FakeMocks;

public class FakeIFullFilePathExistsService : IFullFilePathExistsService
{
	public Task<FullFilePathExistsResultModel> GetFullFilePath(string subPath,
		string beforeFileHashWithoutExtension)
	{
		return Task.FromResult(new FullFilePathExistsResultModel
		{
			IsSuccess = true,
			FullFilePath = subPath,
			IsTempFile = false,
			TempFileFileHashWithExtension = beforeFileHashWithoutExtension + ".jpg"
		});
	}

	public void CleanTemporaryFile(string fileHashWithExtension, bool useTempStorageForInput)
	{
		// do nothing
	}
}
