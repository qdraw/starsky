using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.sync.Helpers;

internal class HasDiskContentOrExistsHelper(IStorage subPathStorage)
{
	internal async Task<(bool hasDiskContentOrExists, string reason)>
		HasDiskContentOrExistsAsync(
			string filePath)
	{
		if ( subPathStorage.ExistFolder(filePath) )
		{
			return ( true, "folder exists on disk" );
		}

		var subDirectories = subPathStorage
			.GetDirectoryRecursive(filePath).ToList();
		if ( subDirectories.Count != 0 )
		{
			return ( true, "subdirectories exist on disk" );
		}

		var filesInFolder = subPathStorage
			.GetAllFilesInDirectory(filePath).ToList();
		if ( filesInFolder.Count != 0 )
		{
			return ( true, "files exist on disk" );
		}

		// Transient IO states can briefly report a folder as missing,
		// retry once before delete.
		await Task.Delay(75);

		if ( subPathStorage.ExistFolder(filePath) )
		{
			return ( true, "folder exists on disk" );
		}

		subDirectories = subPathStorage.GetDirectoryRecursive(filePath).ToList();
		if ( subDirectories.Count != 0 )
		{
			return ( true, "subdirectories exist on disk" );
		}

		filesInFolder = subPathStorage.GetAllFilesInDirectory(filePath).ToList();
		if ( filesInFolder.Count != 0 )
		{
			return ( true, "files exist on disk" );
		}

		return ( false, "folder missing and no content found" );
	}
}
