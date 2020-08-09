using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using starsky.feature.webftppublish.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.webftppublish.Services
{
	[Service(typeof(IFtpService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class FtpService : IFtpService
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _storage;
		private readonly IConsole _console;
		private readonly IWebRequestAbstraction _webRequest;

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
		/// <param name="console"></param>
		/// <param name="webRequest"></param>
		public FtpService(AppSettings appSettings, IStorage storage, IConsole console, 
			IWebRequestAbstraction webRequest)
		{
			_appSettings = appSettings;
			_storage = storage;
			_console = console;
			_webRequest = webRequest;

			var uri = new Uri(_appSettings.WebFtp);
			_appSettingsCredentials = uri.UserInfo.Split(":".ToCharArray());

			// Replace WebFtpNoLogin
			_webFtpNoLogin = $"{uri.Scheme}://{uri.Host}{uri.LocalPath}";
			
			_appSettingsCredentials[0] = HttpUtility.UrlDecode(_appSettingsCredentials[0]);
			_appSettingsCredentials[1] = HttpUtility.UrlDecode(_appSettingsCredentials[1]);
		}

		/// <summary>
		/// Copy all content to the ftp disk
		/// </summary>
		/// <param name="parentDirectory"></param>
		/// <param name="slug"></param>
		/// <param name="copyContent"></param>
		/// <returns>true == success</returns>
		public bool Run(string parentDirectory, string slug, Dictionary<string, bool> copyContent)
		{
			foreach ( var thisDirectory in 
				CreateListOfRemoteDirectories(parentDirectory, slug, copyContent) )
			{
				_console.Write(",");
				if ( !DoesFtpDirectoryExist(thisDirectory) )
				{
					if ( CreateFtpDirectory(thisDirectory) ) continue;
					_console.WriteLine($"Fail > create directory => {_webFtpNoLogin}");
					continue;
				}
			}

			// content of the publication folder
			var copyThisFilesSubPaths = CreateListOfRemoteFiles(copyContent);
			if ( !MakeUpload(parentDirectory, slug, copyThisFilesSubPaths) )
			{
				return false;
			}

			_console.Write("\n");

			return true;
		}

		/// <summary>
		/// Makes a list of containing: the root folder, subfolders to create on the ftp service
		/// make the 1000 and 500 dirs on ftp
		/// </summary>
		/// <param name="parentDirectory"></param>
		/// <param name="slug"></param>
		/// <param name="copyContent"></param>
		/// <returns></returns>
		internal IEnumerable<string> CreateListOfRemoteDirectories(string parentDirectory, 
			string slug, Dictionary<string, bool> copyContent)
		{
			var pushDirectory = _webFtpNoLogin + "/" + slug;

			var createThisDirectories = new List<string>
			{
				_webFtpNoLogin, // <= the base dir
				pushDirectory // <= current log item
			};
			
			foreach ( var copyItem in copyContent.Where(p => p.Value) )
			{
				var parentItems = Breadcrumbs.BreadcrumbHelper(copyItem.Key);
				foreach ( var item in parentItems.Where(p => p != Path.DirectorySeparatorChar.ToString()) )
				{
					if ( _storage.ExistFolder(parentDirectory + item ) )
					{
						createThisDirectories.Add(pushDirectory + "/" + item);
					}
				}
			}
			
			return new HashSet<string>(createThisDirectories).ToList();
		}

		/// <summary>
		/// Makes a list of 'full file paths' of files on disk to copy
		/// </summary>
		/// <returns></returns>
		internal HashSet<string> CreateListOfRemoteFiles(Dictionary<string, bool> copyContent)
		{
			var copyThisFiles = new List<string>();
			foreach ( var copyItem in copyContent.Where(p => p.Value) )
			{
				copyThisFiles.Add("/" + copyItem.Key);
			}
			return new HashSet<string>(copyThisFiles);
		}


		/// <summary>
		/// Preflight + the upload to the service
		/// </summary>
		/// <param name="parentDirectory"></param>
		/// <param name="slug">name</param>
		/// <param name="copyThisFilesSubPaths">list of files (subPath style)</param>
		/// <returns>false = fail</returns>
		private bool MakeUpload( string parentDirectory, string slug, IEnumerable<string> copyThisFilesSubPaths)
		{
			foreach ( var item in copyThisFilesSubPaths )
			{
				const string pathDelimiter = "/";
				var toFtpPath =  PathHelper.RemoveLatestSlash(_webFtpNoLogin) + pathDelimiter +
				                 _appSettings.GenerateSlug(slug,true) + pathDelimiter +
				                 item;

				_console.Write(".");
				if ( Upload(parentDirectory, item, toFtpPath) ) continue;
				_console.WriteLine($"Fail > upload file => {item} {toFtpPath}");
				return false;
			}
			return true;
		}


		/// <summary>
		/// Upload a single file to the ftp service
		/// </summary>
		/// <param name="parentDirectory"></param>
		/// <param name="subPath">on disk</param>
		/// <param name="toFtpPath">ftp path eg ftp://service.nl/drop/test//index.html</param>
		/// <returns></returns>
		private bool Upload(string parentDirectory,  string subPath, string toFtpPath)
		{
			if(!_storage.ExistFile(parentDirectory + subPath)) return false;
			
			FtpWebRequest request =
				(FtpWebRequest)WebRequest.Create(toFtpPath);
			request.Credentials = new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
			request.Method = WebRequestMethods.Ftp.UploadFile;  

			using (Stream fileStream = _storage.ReadStream(parentDirectory + subPath))
			using (Stream ftpStream = request.GetRequestStream())
			{
				fileStream.CopyTo(ftpStream);
			}
			return true;
		}

		/// <summary>
		/// 
		/// @see: https://stackoverflow.com/a/24047971
		/// </summary>
		/// <param name="dirPath"></param>
		/// <returns></returns>
		private bool DoesFtpDirectoryExist(string dirPath)
		{
			try
			{
				FtpWebRequest request = ( FtpWebRequest )WebRequest.Create(dirPath);
				request.Credentials = new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
				request.Method = WebRequestMethods.Ftp.ListDirectory;
				FtpWebResponse response = ( FtpWebResponse )request.GetResponse();
				return true;
			}
			catch ( WebException ex )
			{
				return false;
			}
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
				var requestDir = (FtpWebRequest) _webRequest.Create(directory);
				requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
				requestDir.Credentials = new NetworkCredential(_appSettingsCredentials[0], 
					_appSettingsCredentials[1]);
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
