using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.ArchiveFormats.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.ArchiveFormats;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
[Service(typeof(IZipper), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class Zipper : IZipper
{
	private readonly StorageHostFullPathFilesystem _hostStorage;
	private readonly IWebLogger _logger;

	public Zipper(IWebLogger logger)
	{
		_logger = logger;
		_hostStorage = new StorageHostFullPathFilesystem(logger);
	}

	/// <summary>
	///     Extract zip file to a folder
	/// </summary>
	/// <param name="zipInputFullPath">input e.g: /path/file.zip</param>
	/// <param name="storeZipFolderFullPath">output e.g. /folder/</param>
	/// <returns></returns>
	[SuppressMessage("Usage", "S5042:Make sure that decompressing this archive file is safe")]
	public bool ExtractZip(string zipInputFullPath, string storeZipFolderFullPath)
	{
		if ( !File.Exists(zipInputFullPath) )
		{
			_logger.LogError("[Zipper] Zip file not found: " + zipInputFullPath);
			return false;
		}

		if ( !IsValidZipFile(zipInputFullPath) )
		{
			_logger.LogError("[Zipper] Invalid zip: " + zipInputFullPath);
			return false;
		}

		// Ensures that the last character on the extraction path
		// is the directory separator char. 
		// Without this, a malicious zip file could try to traverse outside the expected
		// extraction path.
		storeZipFolderFullPath = PathHelper.AddBackslash(storeZipFolderFullPath);

		try
		{
			using var archive = ZipFile.OpenRead(zipInputFullPath);
			foreach ( var entry in archive.Entries )
			{
				// Gets the full path to ensure that relative segments are removed.
				var destinationPath =
					Path.GetFullPath(Path.Combine(storeZipFolderFullPath, entry.FullName));

				// Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
				// are case-insensitive.
				if ( !destinationPath.StartsWith(storeZipFolderFullPath,
					    StringComparison.Ordinal) )
				{
					continue;
				}

				// Folders inside zips give sometimes issues
				if ( entry.FullName.EndsWith('/') )
				{
					if ( !_hostStorage.ExistFolder(destinationPath) )
					{
						_hostStorage.CreateDirectory(destinationPath);
					}

					continue;
				}

				try
				{
					entry.ExtractToFile(destinationPath, true);
				}
				catch ( DirectoryNotFoundException )
				{
					Directory.GetParent(destinationPath)!.Create();
					entry.ExtractToFile(destinationPath, true);
				}
				catch ( IOException exception )
				{
					_logger.LogError($"[Zipper] IOException: {zipInputFullPath}", exception);
					return false;
				}
			}
		}
		catch ( InvalidDataException exception )
		{
			_logger.LogError($"[Zipper] Failed to extract {exception} - {zipInputFullPath}",
				exception);
			return false;
		}

		return true;
	}

	public Dictionary<string, byte[]> ExtractZip(byte[] zipped)
	{
		var result = new Dictionary<string, byte[]>();

		if ( !IsValidZipFile(zipped) )
		{
			_logger.LogError("[Zipper] Invalid zip in byte array");
			return result;
		}

		try
		{
			using var memoryStream = new MemoryStream(zipped);
			using var archive = new ZipArchive(memoryStream);
			foreach ( var entry in archive.Entries )
			{
				using var entryStream = entry.Open();
				using var reader = new BinaryReader(entryStream);
				result.Add(entry.FullName, reader.ReadBytes(( int ) entry.Length));
			}

			return result;
		}
		catch ( InvalidDataException exception )
		{
			_logger.LogError($"[Zipper] Failed to extract {exception}", exception);
			return result;
		}
	}

	public static bool IsValidZipFile(string fullFilePath)
	{
		return RetryHelper.Do(CheckIfIsValidZipFile,
			TimeSpan.FromSeconds(1));

		bool CheckIfIsValidZipFile()
		{
			if ( !File.Exists(fullFilePath) )
			{
				return false;
			}

			var buffer = new byte[4];
			using ( var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read) )
			{
				if ( fs.Read(buffer, 0, 4) != 4 )
				{
					return false;
				}
			}

			return IsValidZipFile(buffer);
		}
	}

	/// <summary>
	///     ZIP files start with 'PK\x03\x04'
	/// </summary>
	/// <param name="fileBytes"></param>
	/// <returns></returns>
	public static bool IsValidZipFile(byte[] fileBytes)
	{
		if ( fileBytes.Length < 4 )
		{
			return false;
		}

		// ZIP files start with 'PK\x03\x04'
		return fileBytes[0] == 0x50 &&
		       fileBytes[1] == 0x4B &&
		       fileBytes[2] == 0x03 &&
		       fileBytes[3] == 0x04;
	}


	/// <summary>
	///     To Create the zip file in the storeZipFolderFullPath folder
	///     Skip if zip file already exist
	/// </summary>
	/// <param name="storeZipFolderFullPath">folder to create zip in</param>
	/// <param name="filePaths">list of full file paths</param>
	/// <param name="fileNames">list of filenames</param>
	/// <param name="zipOutputFilename">to name of the zip file (zipHash)</param>
	/// <returns>a zip in the temp folder</returns>
	[SuppressMessage("Usage", "S2325:Make CreateZip a static method")]
	public string CreateZip(string storeZipFolderFullPath, List<string> filePaths,
		List<string> fileNames, string zipOutputFilename)
	{
		var tempFileFullPath = Path.Combine(storeZipFolderFullPath, zipOutputFilename) + ".zip";

		// Has a direct dependency on the filesystem to avoid large content in memory
		if ( File.Exists(tempFileFullPath) )
		{
			return tempFileFullPath;
		}

		var zip = ZipFile.Open(tempFileFullPath, ZipArchiveMode.Create);

		for ( var i = 0; i < filePaths.Count; i++ )
		{
			if ( File.Exists(filePaths[i]) )
			{
				var fileName = fileNames[i];
				zip.CreateEntryFromFile(filePaths[i], fileName);
			}
		}

		zip.Dispose(); // no flush
		return tempFileFullPath;
	}
}
