using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using starsky.foundation.platform.Models;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskywebftpcli.Services
{
	public class FtpService
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _storage;

		/// <summary>
		/// [0] is username, [1] password
		/// </summary>
		private readonly string[] _appSettingsCredentials;

		/// <summary>
		/// eg ftp://service.nl/drop/
		/// </summary>
		private readonly string _webFtpNoLogin;


		/// <summary>
		/// Use ftp://username:password@ftp.service.tld/pushfolder to extract credentials
		/// Encode content using html for @ use %40 for example
		/// </summary>
		/// <param name="appSettings">the location of the settings</param>
		/// <param name="storage">storage provider for source files</param>
		public FtpService(AppSettings appSettings, IStorage storage)
		{
			_appSettings = appSettings;
			_storage = storage;

			var uri = new Uri(_appSettings.WebFtp);
			_appSettingsCredentials = uri.UserInfo.Split(":".ToCharArray());

			// Replace WebFtpNoLogin
			_webFtpNoLogin = $"{uri.Scheme}://{uri.Host}{uri.LocalPath}";
			
			_appSettingsCredentials[0] = HttpUtility.UrlDecode(_appSettingsCredentials[0]);
			_appSettingsCredentials[1] = HttpUtility.UrlDecode(_appSettingsCredentials[1]);
		}

		/// <summary>
		/// Makes a list of containing: the root folder, subfolders to create on the ftp service
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<string> CreateListOfRemoteDirectories()
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

			return createThisDirectories;
		}

		/// <summary>
		/// Makes a list of 'full file paths' of files on disk to copy
		/// </summary>
		/// <returns></returns>
		internal HashSet<string> CreateListOfRemoteFiles()
		{
			// copy content of dir
			var copyThisFiles = new List<string>();
			foreach ( var publishProfile in _appSettings.PublishProfiles )
			{
				switch ( publishProfile.ContentType )
				{
					case TemplateContentType.Jpeg when publishProfile.Copy:
					{
						var files = _storage.GetAllFilesInDirectory(publishProfile.Folder)
							.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
						copyThisFiles.AddRange(files);
						break;
					}
					case TemplateContentType.Html when publishProfile.Copy:
						copyThisFiles.Add(publishProfile.Path);
						break;
					case TemplateContentType.None:
						break;
					case TemplateContentType.MoveSourceFiles:
						break;
				}
			}
			
			// Add PublishedContent (content in main-folder)
			var publishedContent = _storage.GetAllFilesInDirectory("/").ToList();
			copyThisFiles.AddRange(publishedContent);

			return copyThisFiles.ToHashSet();
		}


		/// <summary>
		/// Copy all content to the ftp disk
		/// </summary>
		/// <returns>true == success</returns>
		public bool Run()
		{
			
			foreach ( var thisDirectory in CreateListOfRemoteDirectories() )
			{
				Console.Write(",");
				if ( CreateFtpDirectory(thisDirectory) ) continue;
				Console.WriteLine($"Fail > create directory => {_webFtpNoLogin}");
				return false;
			}

			// content of the publication folder
			var copyThisFilesSubPaths = CreateListOfRemoteFiles();
			if(!MakeUpload(copyThisFilesSubPaths)) return false;

			Console.Write("\n");
			return true;
		}

		/// <summary>
		/// Preflight + the upload to the service
		/// </summary>
		/// <param name="copyThisFilesSubPaths">list of files (subPath style)</param>
		/// <returns>false = fail</returns>
		private bool MakeUpload(IEnumerable<string> copyThisFilesSubPaths)
		{
			foreach ( var item in copyThisFilesSubPaths )
			{
				var toFtpPath =  PathHelper.RemoveLatestSlash(_webFtpNoLogin) + "/" +
				                 _appSettings.GenerateSlug(_appSettings.Name,true) + "/" +
				                 item;

				Console.Write(".");
				if ( Upload(item, toFtpPath) ) continue;
				Console.WriteLine($"Fail > upload file => {item} {toFtpPath}");
				return false;
			}
			return true;
		}


		/// <summary>
		/// Upload a single file to the ftp service
		/// </summary>
		/// <param name="subPath">on disk</param>
		/// <param name="toFtpPath">ftp path eg ftp://service.nl/drop/test//index.html</param>
		/// <returns></returns>
		private bool Upload(string subPath, string toFtpPath)
		{
			if(!_storage.ExistFile(subPath)) return false;
			
			FtpWebRequest request =
				(FtpWebRequest)WebRequest.Create(toFtpPath);
			request.Credentials = new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
			request.Method = WebRequestMethods.Ftp.UploadFile;  

			using (Stream fileStream = _storage.ReadStream(subPath))
			using (Stream ftpStream = request.GetRequestStream())
			{
				fileStream.CopyTo(ftpStream);
			}
			return true;
		}
		
		/// <summary>
		/// Create a directory on the ftp service
		/// </summary>
		/// <param name="directory">ftp path with directory name eg ftp://service.nl/drop</param>
		/// <returns></returns>
		private bool CreateFtpDirectory(string directory) 
		{

			try
			{
				// create the directory
				var requestDir = (FtpWebRequest) WebRequest.Create(directory);
				requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
				requestDir.Credentials = new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
				requestDir.UsePassive = true;
				requestDir.UseBinary = true;
				requestDir.KeepAlive = false;
				var ftpWebResponse = (FtpWebResponse)requestDir.GetResponse();
				var ftpStream = ftpWebResponse.GetResponseStream();

				ftpStream?.Close();
				ftpWebResponse.Close();

				return true;
			}
			catch (WebException ex)
			{
				var ftpWebResponse = (FtpWebResponse)ex.Response;
				ftpWebResponse.Close();
				return ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable;
			}
		}
		
	}
}
