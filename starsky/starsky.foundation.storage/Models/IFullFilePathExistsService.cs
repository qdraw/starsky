using System.Threading.Tasks;

namespace starsky.foundation.storage.Models;

public interface IFullFilePathExistsService
{
	/// <summary>
	///     Get a full file path, if needed copy it to a temp folder
	/// </summary>
	/// <param name="subPath">subPath style</param>
	/// <returns></returns>
	Task<FullFilePathExistsResultModel> GetFullFilePath(string subPath,
		string beforeFileHashWithoutExtension);

	void CleanTemporaryFile(string fileHashWithExtension, bool useTempStorageForInput);
}
