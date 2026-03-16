using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.sync.Helpers;

/*
 This helper lives in the sync layer (not inside IStorage) on purpose.
 Reasons:
 1) Transient I/O / TOCTOU: filesystem checks are racy – a folder can briefly
    appear missing while a host or OS is still finalizing writes. If we simply
    relied on a single `ExistFolder` / listing call inside storage, a transient
    false-negative could cause an accidental deletion from the DB.

 2) Separation of concerns: low-level storage should expose accurate probes
    (ExistFolder, GetDirectoryRecursive, GetAllFilesInDirectory). The sync
    logic is responsible for the higher-level policy "safe to delete?"
    which includes retrying a short while before taking destructive action.

 3) Performance vs correctness: we keep the retry lightweight (75ms) to
    avoid expensive full-file scans while protecting against common transient
    race windows seen on removable drives, network mounts and some OSes.

 Tests: there are unit tests that exercise the immediate and retry branches
 (including simulated transient appearances). Keep the retry small and
 deterministic to avoid test flakiness.
*/
internal class HasDiskContentOrExistsHelper(IStorage subPathStorage)
{
	internal async Task<(bool hasDiskContentOrExists, string reason, bool logAsDebug)>
		HasDiskContentOrExistsAsync(
			string filePath)
	{
		if ( subPathStorage.ExistFolder(filePath) )
		{
			return ( true, "folder exists on disk", true );
		}

		var subDirectories = subPathStorage
			.GetDirectoryRecursive(filePath).ToList();
		if ( subDirectories.Count != 0 )
		{
			return ( true, "subdirectories exist on disk", false );
		}

		var filesInFolder = subPathStorage
			.GetAllFilesInDirectory(filePath).ToList();
		if ( filesInFolder.Count != 0 )
		{
			return ( true, "files exist on disk", false );
		}

		// Transient IO states can briefly report a folder as missing.
		// We retry once with a short delay before deciding to delete.
		// Why here (sync layer) and not in storage? Because this is a
		// sync policy: storage should return raw state, sync decides on
		// the retry semantics and deletion policy (TOCTOU mitigation).
		await Task.Delay(75);

		if ( subPathStorage.ExistFolder(filePath) )
		{
			return ( true, "folder exists on disk", false );
		}

		subDirectories = subPathStorage.GetDirectoryRecursive(filePath).ToList();
		if ( subDirectories.Count != 0 )
		{
			return ( true, "subdirectories exist on disk", false );
		}

		filesInFolder = subPathStorage.GetAllFilesInDirectory(filePath).ToList();
		if ( filesInFolder.Count != 0 )
		{
			return ( true, "files exist on disk", false );
		}

		return ( false, "folder missing and no content found", false );
	}
}
