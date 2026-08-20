using System;
using System.IO;

namespace starsky.foundation.platform.Models;

public class AppSettingsCreateDefaultFolders(
	string baseDirectoryProject,
	string thumbnailTempFolder,
	string storageFolder,
	string tempFolder,
	string dependenciesFolder)
{
	/// <summary>
	///     @see: https://tomasherceg.com/blog/post/
	///     azure-app-service-cannot-create-directories-and-write-to-filesystem-when-deployed-using-azure-devops
	/// </summary>
	public void CreateDefaultFolders()
	{
		CreateDefaultFoldersIfNotExists(() =>
		{
			if ( !Directory.Exists(baseDirectoryProject) )
			{
				Directory.CreateDirectory(baseDirectoryProject);
			}
		});

		// Cache for thumbs
		CreateDefaultFoldersIfNotExists(() =>
		{
			if ( !Directory.Exists(thumbnailTempFolder) )
			{
				Directory.CreateDirectory(thumbnailTempFolder);
			}
		});

		// default location to store source images. you should change this
		CreateDefaultFoldersIfNotExists(() =>
		{
			if ( !Directory.Exists(storageFolder) )
			{
				Directory.CreateDirectory(storageFolder);
			}
		});

		// may be cleaned after restart (not implemented)
		CreateDefaultFoldersIfNotExists(() =>
		{
			if ( !Directory.Exists(tempFolder) )
			{
				Directory.CreateDirectory(tempFolder);
			}
		});

		CreateDefaultFoldersIfNotExists(() =>
		{
			if ( !Directory.Exists(dependenciesFolder) )
			{
				Directory.CreateDirectory(dependenciesFolder);
			}
		});
	}

	private static void CreateDefaultFoldersIfNotExists(
		CreateDefaultFoldersDelegate createDefaultFoldersDelegate)
	{
		try
		{
			createDefaultFoldersDelegate();
		}
		catch ( FileNotFoundException e )
		{
			Console.WriteLine($"> Not allowed to create default folders: {e}");
		}
		catch ( UnauthorizedAccessException e )
		{
			Console.WriteLine($"> Not allowed to create default folders: {e}");
		}
	}

	private delegate void CreateDefaultFoldersDelegate();
}
