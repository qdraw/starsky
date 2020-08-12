using System.IO;
using System.Net;

namespace starsky.feature.webftppublish.FtpAbstractions.Interfaces
{
	public interface IFtpWebRequest
	{
		// expose the members you need
		string Method { get; set; }
		NetworkCredential Credentials { get; set; }
		bool UsePassive { get; set; }
		bool UseBinary { get; set; }
		bool KeepAlive { get; set; }

		IFtpWebResponse GetResponse();
		Stream GetRequestStream();
	}
}
