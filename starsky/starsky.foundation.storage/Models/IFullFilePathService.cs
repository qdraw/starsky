using System.Threading.Tasks;

namespace starsky.foundation.storage.Models;

public interface IFullFilePathService
{
	/// <summary>
	///     Get a full file path, if needed copy it to a temp folder
	/// </summary>
	/// <param name="subPath">subPath style</param>
	/// <returns>(fullFilePath, isTempFile, fileHashWithExtension)</returns>
	Task<(string, bool, string)> GetFullFilePath(string subPath,
		string beforeFileHash);
}
