using System;
using System.IO;
using System.Net;
using starskycore.Models;

namespace starskywebftpcli.Services
{
	public class FtpService
	{
		private readonly AppSettings _appSettings;
		private Uri _appSettingsUri;
		private string[] _appSettingsCredentials;

		public FtpService(AppSettings appSettings)
		{
			_appSettings = appSettings;
			_appSettingsUri = new Uri (_appSettings.WebFtp);
			_appSettingsCredentials = _appSettingsUri.UserInfo.Split(":".ToCharArray());

		}
		public void Upload()
		{
			
			Uri uriAddress = new Uri (_appSettings.WebFtp);
			     
			FtpWebRequest request =
				(FtpWebRequest)WebRequest.Create("ftp://ftp.example.com/remote/path/file.zip");
			request.Credentials = new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
			request.Method = WebRequestMethods.Ftp.UploadFile;  

			using (Stream fileStream = File.OpenRead(@"C:\local\path\file.zip"))
			using (Stream ftpStream = request.GetRequestStream())
			{
				fileStream.CopyTo(ftpStream);
			}	
		}
		
		private bool CreateFtpDirectory(string directory) {

			try
			{
				//create the directory
				FtpWebRequest requestDir = (FtpWebRequest)WebRequest.Create(new Uri(directory));
				requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
				requestDir.Credentials = new NetworkCredential("username", "password");
				requestDir.UsePassive = true;
				requestDir.UseBinary = true;
				requestDir.KeepAlive = false;
				FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
				Stream ftpStream = response.GetResponseStream();

				ftpStream.Close();
				response.Close();

				return true;
			}
			catch (WebException ex)
			{
				FtpWebResponse response = (FtpWebResponse)ex.Response;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
				{
					response.Close();
					return true;
				}
				response.Close();
				return false;
			}
		}
		
	}
}
