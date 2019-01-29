using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;

namespace starskywebftpcli.Services
{
	public class FtpService
	{
		private readonly AppSettings _appSettings;
		private readonly string[] _appSettingsCredentials;

		private readonly string _webFtpNoLogin;
		
		public FtpService(AppSettings appSettings)
		{
			_appSettings = appSettings;
			var uri = new Uri(_appSettings.WebFtp);
			_appSettingsCredentials = uri.UserInfo.Split(":".ToCharArray());

			// Replace WebFtpNoLogin
			_webFtpNoLogin = $"{uri.Scheme}://{uri.Host}{uri.LocalPath}";
			
			_appSettingsCredentials[0] = HttpUtility.UrlDecode(_appSettingsCredentials[0]);
			_appSettingsCredentials[1] = HttpUtility.UrlDecode(_appSettingsCredentials[1]);
		}

		public bool Run()
		{
			var pushDirectory = _webFtpNoLogin + "/" + _appSettings.GenerateSlug(_appSettings.Name,true);

			var createThisDirectories = new List<string>
			{
				_webFtpNoLogin, // <= the base dir
				pushDirectory // <= current log item
			};

			// make the 1000 and 500 dirs on ftp
			foreach ( var publishProfile in _appSettings.PublishProfiles )
			{
				if ( publishProfile.ContentType == TemplateContentType.Jpeg )
				{
					createThisDirectories.Add(pushDirectory + "/" + publishProfile.Folder);
				}
			}
			
			foreach ( var thisDirectory in createThisDirectories )
			{
				if ( CreateFtpDirectory(thisDirectory) ) continue;
				Console.WriteLine($"Fail > create directory => {_webFtpNoLogin}");
				return false;
			}
			
			
			// copy content of dir
			var copyThisFiles = new List<string>();
			foreach ( var publishProfile in _appSettings.PublishProfiles )
			{
				if ( publishProfile.ContentType == TemplateContentType.Jpeg  && publishProfile.Copy)
				{					

					var folderPath = Path.Combine(_appSettings.StorageFolder,publishProfile.Folder);
					copyThisFiles.AddRange(Files.GetFilesInDirectory(folderPath));
				}
			}

			var subPathCopyThisFiles = _appSettings.RenameListItemsToDbStyle(copyThisFiles);

			
			return true;
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
				FtpWebRequest requestDir = (FtpWebRequest) FtpWebRequest.Create(directory);
				requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
				requestDir.Credentials = new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
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

				response.Close();

				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
				{
					return true;
				}
				return false;
			}
		}
		
	}
}
