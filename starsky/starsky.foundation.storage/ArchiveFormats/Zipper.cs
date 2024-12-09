using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.ArchiveFormats;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class Zipper
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

		// Ensures that the last character on the extraction path
		// is the directory separator char. 
		// Without this, a malicious zip file could try to traverse outside of the expected
		// extraction path.
		storeZipFolderFullPath = PathHelper.AddBackslash(storeZipFolderFullPath);
		using ( var archive = ZipFile.OpenRead(zipInputFullPath) )
		{
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
			}
		}

		return true;
	}

	public static Dictionary<string, byte[]> ExtractZip(byte[] zipped)
	{
		using var memoryStream = new MemoryStream(zipped);
		using var archive = new ZipArchive(memoryStream);
		var result = new Dictionary<string, byte[]>();
		foreach ( var entry in archive.Entries )
		{
			// only the first item
			using var entryStream = entry.Open();
			using var reader = new BinaryReader(entryStream);
			result.Add(entry.FullName, reader.ReadBytes(( int ) entry.Length));
		}

		return result;
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
