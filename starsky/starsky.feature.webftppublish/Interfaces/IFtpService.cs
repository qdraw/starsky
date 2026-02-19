using System.Collections.Generic;

namespace starsky.feature.webftppublish.Interfaces;

public interface IFtpService
{
	bool Run(string parentDirectoryOrZipFile, string slug,
		Dictionary<string, bool> copyContent);
}
