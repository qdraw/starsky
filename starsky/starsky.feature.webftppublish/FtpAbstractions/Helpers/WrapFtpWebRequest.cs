using System.IO;
using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;

namespace starsky.feature.webftppublish.FtpAbstractions.Helpers
{
	public class WrapFtpWebRequest : IFtpWebRequest
	{
		private readonly FtpWebRequest _request;

		public WrapFtpWebRequest(FtpWebRequest request)
		{
			_request = request;
		}
		
		/// <summary>
		/// <para>
		/// Selects FTP command to use. WebRequestMethods.Ftp.DownloadFile is default.
		/// Not allowed to be changed once request is started.
		/// </para>
		/// </summary>
		public string Method
		{
			get => _request.Method;
			set => _request.Method = value;
		}

		/// <summary>
		/// <para>Used for clear text authentication with FTP server</para>
		/// </summary>
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
		
		/// <summary>
		/// <para>True by default, false allows transmission using text mode</para>
		/// </summary>
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

		/// <summary>
		/// Used to query for the Response of an FTP request
		/// </summary>
		/// <returns>Wrapper response</returns>
		public IFtpWebResponse GetResponse()
		{
			return new WrapFtpWebResponse((FtpWebResponse)_request.GetResponse());
		}

		/// <summary>
		/// <para>Used to query for the Request stream of an FTP Request</para>
		/// </summary>
		public Stream GetRequestStream()
		{
			return _request.GetRequestStream();
		}
	}

}
