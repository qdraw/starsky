using System;
using System.IO;
using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIFtpWebRequestFactory : IFtpWebRequestFactory
{
	public IFtpWebRequest Create(string uri)
	{
		return new FakeIFtpWebRequest(uri);
	}
}

public class FakeIFtpWebRequest : IFtpWebRequest
{
	public FakeIFtpWebRequest(string uri)
	{
		Uri = uri;
	}

	private string Uri { get; }

	public string Method { get; set; } = string.Empty;

	public NetworkCredential Credentials { get; set; } = new();

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

	public Stream GetRequestStream()
	{
		return new MemoryStream();
	}
}

public class FakeIFtpWebResponse : IFtpWebResponse
{
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(true);
	}


	public Stream GetResponseStream()
	{
		return new MemoryStream();
	}

	protected virtual void Dispose(bool disposing)
	{
		// do nothing
	}
}
