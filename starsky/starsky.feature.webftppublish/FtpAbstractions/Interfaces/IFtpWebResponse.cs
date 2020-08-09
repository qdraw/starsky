using System;
using System.IO;

namespace starsky.feature.webftppublish.FtpAbstractions.Interfaces
{
	public interface IFtpWebResponse : IDisposable
	{
		// expose the members you need
		Stream GetResponseStream();
	}
}
