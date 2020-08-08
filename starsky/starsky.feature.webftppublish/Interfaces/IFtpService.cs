using System;
using System.Collections.Generic;

namespace starsky.feature.webftppublish.Interfaces
{
	public interface IFtpService
	{
		bool Run(string parentDirectory, string slug, List<Tuple<string, bool>> copyContent);
	}
}
