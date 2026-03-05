using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.webftppublish.Models;

namespace starsky.feature.webftppublish.Interfaces;

public interface IFtpService
{
	Task<FtpPublishManifestModel?> IsValidZipOrFolder(string inputFullFileDirectoryOrZip);

	PublishServiceResultModel Run(string parentDirectoryOrZipFile, string profileId, string slug,
		Dictionary<string, bool> copyContent);
}
