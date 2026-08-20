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
	private void CreateDefaultFolders()
	{
		if ( !Directory.Exists(baseDirectoryProject) )
		{
			Directory.CreateDirectory(baseDirectoryProject);
		}

		// Cache for thumbs
		if ( !Directory.Exists(thumbnailTempFolder) )
		{
			Directory.CreateDirectory(thumbnailTempFolder);
		}

		// default location to store source images. you should change this
		if ( !Directory.Exists(storageFolder) )
		{
			Directory.CreateDirectory(storageFolder);
		}

		// may be cleaned after restart (not implemented)
		if ( !Directory.Exists(tempFolder) )
		{
			Directory.CreateDirectory(tempFolder);
		}

		if ( !Directory.Exists(dependenciesFolder) )
		{
			Directory.CreateDirectory(dependenciesFolder);
		}
	}

	public void CreateDefaultFoldersIfNotExists()
	{
		try
		{
			CreateDefaultFolders();
		}
		catch ( FileNotFoundException e )
		{
			Console.WriteLine($"> Not allowed to create default folders: {e}");
		}
		catch ( IOException e )
		{
			Console.WriteLine($"> Not allowed to create default folders: {e}");
		}
	}
}
