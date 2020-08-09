using System.IO;
using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIFtpWebRequestFactory : IFtpWebRequestFactory
	{
		public IFtpWebRequest Create(string uri)
		{
			return new FakeIFtpWebRequest(uri);
		}
	}

	public class FakeIFtpWebRequest : IFtpWebRequest
	{
		private string Uri { get; set; }
		public FakeIFtpWebRequest(string uri)
		{
			Uri = uri;
		}
		public string Method { get; set; }
		public NetworkCredential Credentials { get; set; }
		public bool UsePassive { get; set; }
		public bool UseBinary { get; set; }
		public bool KeepAlive { get; set; }

		public IFtpWebResponse GetResponse()
		{
			if ( Uri == "/web-exception" )
			{
				throw new WebException();
			}
			return new FakeIFtpWebResponse();
		}
	}

	public class FakeIFtpWebResponse : IFtpWebResponse
	{
		public void Dispose()
		{
			// done
		}

		public Stream GetResponseStream()
		{
			return new MemoryStream();
		}
	} 
}
