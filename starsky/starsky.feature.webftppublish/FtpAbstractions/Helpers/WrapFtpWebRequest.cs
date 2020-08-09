using System.IO;
using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Helpers;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;

namespace starsky.feature.webftppublish.FtpAbstractions
{
	public class WrapFtpWebRequest : IFtpWebRequest
	{
		private readonly FtpWebRequest _request;

		public WrapFtpWebRequest(FtpWebRequest request)
		{
			_request = request;
		}

		public string Method
		{
			get => _request.Method;
			set => _request.Method = value;
		}

		public NetworkCredential Credentials
		{
			get => null;
			set => _request.Credentials = value;
		}

		public bool UsePassive 
		{
			get => _request.UsePassive;
			set => _request.UsePassive = value;
		}
		public bool UseBinary 
		{
			get => _request.UseBinary;
			set => _request.UseBinary = value;
		}
		public bool KeepAlive
		{
			get => _request.KeepAlive;
			set => _request.KeepAlive = value;
		}

		public IFtpWebResponse GetResponse()
		{
			return new WrapFtpWebResponse((FtpWebResponse)_request.GetResponse());
		}

		public Stream GetRequestStream()
		{
			return _request.GetRequestStream();
		}
	}

}
