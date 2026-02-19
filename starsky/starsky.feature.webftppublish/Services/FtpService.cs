using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;
using starsky.feature.webftppublish.Interfaces;
using starsky.feature.webftppublish.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Helpers.Slug;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.webftppublish.Services;

[Service(typeof(IFtpService), InjectionLifetime = InjectionLifetime.Scoped)]
public class FtpService : IFtpService
{
	/// <summary>
	///     [0] is username, [1] password
	/// </summary>
	private readonly string[] _appSettingsCredentials;

	private readonly IConsole _console;
	private readonly IStorage _storage;

	/// <summary>
	///     eg ftp://service.nl/drop/
	/// </summary>
	private readonly string _webFtpNoLogin;

	private readonly IFtpWebRequestFactory _webRequest;
	private readonly IWebLogger _logger;


	/// <summary>
	///     Use ftp://username:password@ftp.service.tld/pushfolder to extract credentials
	///     Encode content using html for @ use %40 for example
	/// </summary>
	/// <param name="appSettings">the location of the settings</param>
	/// <param name="storage">storage provider for source files</param>
	/// <param name="console"></param>
	/// <param name="webRequest"></param>
	public FtpService(AppSettings appSettings, IStorage storage, IConsole console,
		IFtpWebRequestFactory webRequest, IWebLogger logger)
	{
		_storage = storage;
		_console = console;
		_webRequest = webRequest;
		_logger = logger;

		var uri = new Uri(appSettings.WebFtp);
		_appSettingsCredentials = uri.UserInfo.Split(":".ToCharArray());

		// Replace WebFtpNoLogin
		_webFtpNoLogin = $"{uri.Scheme}://{uri.Host}{uri.LocalPath}";

		_appSettingsCredentials[0] = HttpUtility.UrlDecode(_appSettingsCredentials[0]);
		_appSettingsCredentials[1] = HttpUtility.UrlDecode(_appSettingsCredentials[1]);
	}

	public async Task<FtpPublishManifestModel?> IsValidZipOrFolder(
		string inputFullFileDirectoryOrZip)
	{
		if ( string.IsNullOrWhiteSpace(inputFullFileDirectoryOrZip) )
		{
			_logger.LogError("Please use the -p to add a path first");
			return null;
		}

		var inputPathType = _storage.IsFolderOrFile(inputFullFileDirectoryOrZip);

		switch ( inputPathType )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				_logger.LogError($"Folder location {inputFullFileDirectoryOrZip} " +
				                 $"is not found \nPlease try the `-h` command to get help ");
				return null;
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
			{
				var settingsFullFilePath =
					Path.Combine(inputFullFileDirectoryOrZip, "_settings.json");
				if ( _storage.ExistFile(settingsFullFilePath) )
				{
					return await
						new DeserializeJson(_storage).ReadAsync<FtpPublishManifestModel>(
							settingsFullFilePath);
				}

				_logger.LogError($"Please run 'starskywebhtmlcli' " +
				                 $"first to generate a settings file");
				return null;
			}
			case FolderOrFileModel.FolderOrFileTypeList.File:
				if ( !string.Equals(Path.GetExtension(inputFullFileDirectoryOrZip), ".zip",
					    StringComparison.OrdinalIgnoreCase) )
				{
					return null;
				}

				var zipFirstByteStream = _storage.ReadStream(inputFullFileDirectoryOrZip, 10);
				if ( !Zipper.IsValidZipFile(zipFirstByteStream) )
				{
					_logger.LogError(
						$"Zip file is invalid or unreadable {inputFullFileDirectoryOrZip}");
					return null;
				}

				var manifest =
					new Zipper(_logger).ExtractZipEntry(inputFullFileDirectoryOrZip,
						"_settings.json");
				if ( manifest == null )
				{
					return null;
				}

				var result = JsonSerializer.Deserialize<FtpPublishManifestModel>(manifest);
				return result;
			default:
				return null;
		}
	}

	private ExtractZipResultModel ExtractZip(string parentDirectoryOrZipFile)
	{
		var existFolder = _storage.ExistFolder(parentDirectoryOrZipFile);
		if ( existFolder )
		{
			return new ExtractZipResultModel
			{
				FullFileFolderPath = parentDirectoryOrZipFile,
				RemoveFolderAfterwards = false,
				IsError = false
			};
		}

		var existFile = _storage.ExistFile(parentDirectoryOrZipFile);
		if ( !existFile )
		{
			return new ExtractZipResultModel
			{
				FullFileFolderPath = parentDirectoryOrZipFile, IsError = true
			};
		}

		var parentFolderTempPath = Path.Combine(Path.GetTempPath(), "starsky-webftp",
			Path.GetFileNameWithoutExtension(parentDirectoryOrZipFile) + "_" +
			Guid.NewGuid().ToString("N"));
		_storage.CreateDirectory(parentFolderTempPath);

		var zipper = new Zipper(new WebLogger());
		if ( zipper.ExtractZip(parentDirectoryOrZipFile, parentFolderTempPath) )
		{
			return new ExtractZipResultModel
			{
				FullFileFolderPath = parentFolderTempPath,
				RemoveFolderAfterwards = true,
				IsError = false
			};
		}

		_logger.LogError($"Zip extract failed {parentDirectoryOrZipFile}");
		return new ExtractZipResultModel
		{
			FullFileFolderPath = parentDirectoryOrZipFile, IsError = true
		};
	}

	/// <summary>
	///     Copy all content to the ftp disk
	/// </summary>
	/// <param name="parentDirectoryOrZipFile"></param>
	/// <param name="slug"></param>
	/// <param name="copyContent"></param>
	/// <returns>true == success</returns>
	public bool Run(string parentDirectoryOrZipFile, string slug,
		Dictionary<string, bool> copyContent)
	{
		var resultModel = ExtractZip(parentDirectoryOrZipFile);
		if ( resultModel.IsError )
		{
			return false;
		}

		foreach ( var thisDirectory in
		         CreateListOfRemoteDirectories(resultModel.FullFileFolderPath, slug, copyContent) )
		{
			_console.Write(",");
			if ( DoesFtpDirectoryExist(thisDirectory) )
			{
				continue;
			}

			if ( CreateFtpDirectory(thisDirectory) )
			{
				continue;
			}

			_console.WriteLine($"Fail > create directory => {_webFtpNoLogin}");
		}

		// content of the publication folder
		var copyThisFilesSubPaths = CreateListOfRemoteFiles(copyContent);
		if ( !MakeUpload(resultModel.FullFileFolderPath, slug, copyThisFilesSubPaths) )
		{
			return false;
		}

		if ( resultModel.RemoveFolderAfterwards )
		{
			_storage.FolderDelete(resultModel.FullFileFolderPath);
		}

		_console.Write("\n");

		return true;
	}

	/// <summary>
	///     Makes a list of containing: the root folder, subfolders to create on the ftp service
	///     make the 1000 and 500 dirs on ftp
	/// </summary>
	/// <param name="parentDirectory"></param>
	/// <param name="slug"></param>
	/// <param name="copyContent"></param>
	/// <returns></returns>
	[SuppressMessage("Usage", "S3267:Loops should be simplified with LINQ expressions ")]
	internal IEnumerable<string> CreateListOfRemoteDirectories(string parentDirectory,
		string slug, Dictionary<string, bool> copyContent)
	{
		var pushDirectory = _webFtpNoLogin + "/" + slug;

		var createThisDirectories = new List<string>
		{
			_webFtpNoLogin, // <= the base dir
			pushDirectory // <= current log item
		};

		// ReSharper disable once LoopCanBeConvertedToQuery
		foreach ( var copyItem in copyContent.Where(p => p.Value) )
		{
			var parentItems = Breadcrumbs.BreadcrumbHelper(copyItem.Key);
			foreach ( var item in parentItems.Where(p =>
				         p != Path.DirectorySeparatorChar.ToString()) )
			{
				if ( _storage.ExistFolder(parentDirectory + item) )
				{
					createThisDirectories.Add(pushDirectory + "/" + item);
				}
			}
		}

		return new HashSet<string>(createThisDirectories).ToList();
	}

	/// <summary>
	///     Makes a list of 'full file paths' of files on disk to copy
	/// </summary>
	/// <returns></returns>
	internal static HashSet<string> CreateListOfRemoteFiles(
		Dictionary<string, bool> copyContent)
	{
		var copyThisFiles = copyContent
			.Where(p => p.Value)
			.Select(copyItem => "/" + copyItem.Key).ToList();
		return new HashSet<string>(copyThisFiles);
	}


	/// <summary>
	///     Preflight + the upload to the service
	/// </summary>
	/// <param name="parentDirectory"></param>
	/// <param name="slug">name</param>
	/// <param name="copyThisFilesSubPaths">list of files (subPath style)</param>
	/// <returns>false = fail</returns>
	internal bool MakeUpload(string parentDirectory, string slug,
		IEnumerable<string> copyThisFilesSubPaths)
	{
		foreach ( var item in copyThisFilesSubPaths )
		{
			const string pathDelimiter = "/";
			var toFtpPath = PathHelper.RemoveLatestSlash(_webFtpNoLogin) + pathDelimiter +
			                GenerateSlugHelper.GenerateSlug(slug, true) + pathDelimiter +
			                item;

			_console.Write(".");

			bool LocalUpload()
			{
				return Upload(parentDirectory, item, toFtpPath);
			}

			RetryHelper.Do(LocalUpload, TimeSpan.FromSeconds(10));

			if ( _storage.ExistFile(parentDirectory + item) )
			{
				continue;
			}

			_console.WriteLine($"Fail > upload file => {item} {toFtpPath}");
			return false;
		}

		return true;
	}


	/// <summary>
	///     Upload a single file to the ftp service
	/// </summary>
	/// <param name="parentDirectory"></param>
	/// <param name="subPath">on disk</param>
	/// <param name="toFtpPath">ftp path eg ftp://service.nl/drop/test//index.html</param>
	/// <returns></returns>
	private bool Upload(string parentDirectory, string subPath, string toFtpPath)
	{
		if ( !_storage.ExistFile(parentDirectory + subPath) )
		{
			return false;
		}

		var request = _webRequest.Create(toFtpPath);
		request.Credentials =
			new NetworkCredential(_appSettingsCredentials[0], _appSettingsCredentials[1]);
		request.Method = WebRequestMethods.Ftp.UploadFile;

		using ( var fileStream = _storage.ReadStream(parentDirectory + subPath) )
		using ( var ftpStream = request.GetRequestStream() )
		{
			fileStream.CopyTo(ftpStream);
		}

		return true;
	}

	/// <summary>
	///     Check if folder exist on ftp drive
	///     @see: https://stackoverflow.com/a/24047971
	/// </summary>
	/// <param name="dirPath">directory may contain slash</param>
	/// <returns>exist or not</returns>
	internal bool DoesFtpDirectoryExist(string dirPath)
	{
		try
		{
			var request = _webRequest.Create(dirPath);
			request.Credentials = new NetworkCredential(_appSettingsCredentials[0],
				_appSettingsCredentials[1]);
			request.Method = WebRequestMethods.Ftp.ListDirectory;
			request.GetResponse();
			return true;
		}
		catch ( WebException )
		{
			return false;
		}
	}

	/// <summary>
	///     Create a directory on the ftp service
	/// </summary>
	/// <param name="directory">ftp path with directory name eg ftp://service.nl/drop</param>
	/// <returns></returns>
	internal bool CreateFtpDirectory(string directory)
	{
		try
		{
			// create the directory
			var requestDir = _webRequest.Create(directory);
			requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
			requestDir.Credentials = new NetworkCredential(_appSettingsCredentials[0],
				_appSettingsCredentials[1]);
			requestDir.UsePassive = true;
			requestDir.UseBinary = true;
			requestDir.KeepAlive = false;
			var ftpWebResponse = requestDir.GetResponse();
			var ftpStream = ftpWebResponse.GetResponseStream();

			ftpStream?.Close();
			ftpWebResponse.Dispose();

			return true;
		}
		catch ( WebException ex )
		{
			var ftpWebResponse = ( FtpWebResponse? ) ex.Response;
			ftpWebResponse?.Close();
			return ftpWebResponse?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable;
		}
	}
}
