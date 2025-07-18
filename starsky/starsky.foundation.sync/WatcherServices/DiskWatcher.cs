using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.sync.WatcherInterfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.sync.WatcherServices;

/// <summary>
///     Service is created only once, and used everywhere
/// </summary>
[Service(typeof(IDiskWatcher), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class DiskWatcher : IDiskWatcher, IDisposable
{
	private readonly IQueueProcessor _queueProcessor;
	private readonly IWebLogger _webLogger;
	private IFileSystemWatcherWrapper _fileSystemWatcherWrapper;

	public DiskWatcher(IFileSystemWatcherWrapper fileSystemWatcherWrapper,
		IServiceScopeFactory scopeFactory)
	{
		_fileSystemWatcherWrapper = fileSystemWatcherWrapper;
		var serviceProvider = scopeFactory.CreateScope().ServiceProvider;
		_webLogger = serviceProvider.GetRequiredService<IWebLogger>();
		_queueProcessor =
			new QueueProcessor(scopeFactory, new SyncWatcherConnector(scopeFactory).Sync);
	}

	internal DiskWatcher(
		IFileSystemWatcherWrapper fileSystemWatcherWrapper,
		IWebLogger logger, IQueueProcessor queueProcessor)
	{
		_fileSystemWatcherWrapper = fileSystemWatcherWrapper;
		_webLogger = logger;
		_queueProcessor = queueProcessor;
	}

	/// <summary>
	///     @see: https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netcore-3.1
	/// </summary>
	public void Watcher(string fullFilePath)
	{
		if ( !Directory.Exists(fullFilePath) )
		{
			_webLogger.LogError(
				$"[DiskWatcher] FAIL can't find directory: {fullFilePath} so watcher is not started");
			return;
		}

		_webLogger.LogInformation($"[DiskWatcher] started {fullFilePath}" +
		                          $"{DateTimeDebug()}");

		// Create a new FileSystemWatcher and set its properties.

		_fileSystemWatcherWrapper.Path = fullFilePath;
		_fileSystemWatcherWrapper.Filter = "*";
		_fileSystemWatcherWrapper.IncludeSubdirectories = true;
		_fileSystemWatcherWrapper.NotifyFilter = NotifyFilters.FileName
		                                         | NotifyFilters.DirectoryName
		                                         | NotifyFilters.Attributes
		                                         | NotifyFilters.Size
		                                         | NotifyFilters.LastWrite
		                                         | NotifyFilters.CreationTime
		                                         | NotifyFilters.Security;

		// Watch for changes in LastAccess and LastWrite times, and
		// the renaming of files or directories.

		// handle many file system events quickly
		_fileSystemWatcherWrapper.InternalBufferSize = 10 * 1024; // 10 KB - default = 4096 / 4 KB

		// Add event handlers.
		_fileSystemWatcherWrapper.Created += OnChanged;
		_fileSystemWatcherWrapper.Changed += OnChanged;
		_fileSystemWatcherWrapper.Deleted += OnChanged;
		_fileSystemWatcherWrapper.Renamed += OnRenamed;
		_fileSystemWatcherWrapper.Error += OnError;

		// Begin watching.
		_fileSystemWatcherWrapper.EnableRaisingEvents = true;
	}

	public void Dispose()
	{
		_fileSystemWatcherWrapper.EnableRaisingEvents = false;
		_fileSystemWatcherWrapper.Dispose();
	}

	private void OnError(object source, ErrorEventArgs e)
	{
		//  Show that an error has been detected.
		_webLogger.LogError(e.GetException(),
			"[DiskWatcher] The FileSystemWatcher has an error (catch-ed) - next: retry " +
			$"{DateTimeDebug()}");
		_webLogger.LogError("[DiskWatcher] (catch-ed) " + e.GetException().Message);

		//  Give more information if the error is due to an internal buffer overflow.
		if ( e.GetException().GetType() == typeof(InternalBufferOverflowException) )
		{
			//  This can happen if Windows is reporting many file system events quickly 
			//  and internal buffer of the  FileSystemWatcher is not large enough to handle this
			//  rate of events. The InternalBufferOverflowException error informs the application
			//  that some of the file system events are being lost.
			_webLogger.LogError(e.GetException(),
				"[DiskWatcher] The file system watcher experienced an internal buffer overflow ");
		}

		// when test dont retry
		if ( e.GetException().Message == "test" )
		{
			return;
		}

		// When fail it should try it again
		Retry(new BufferingFileSystemWatcher(_fileSystemWatcherWrapper.Path));
	}

	/// <summary>
	///     @see: https://www.codeguru.com/dotnet/filesystemwatcher%EF%BF%BDwhy-does-it-stop-working/
	/// </summary>
	internal bool Retry(IFileSystemWatcherWrapper fileSystemWatcherWrapper,
		int numberOfTries = 20, int milliSecondsTimeout = 5000)
	{
		_webLogger.LogInformation("[DiskWatcher] next retry " +
		                          $"{DateTimeDebug()}");
		var path = _fileSystemWatcherWrapper.Path;

		_fileSystemWatcherWrapper.Dispose();
		_fileSystemWatcherWrapper = fileSystemWatcherWrapper;

		var i = 0;
		while ( !_fileSystemWatcherWrapper.EnableRaisingEvents && i < numberOfTries )
		{
			try
			{
				// This will throw an error at the
				// watcher.NotifyFilter line if it can't get the path.
				Watcher(path);
				if ( _fileSystemWatcherWrapper.EnableRaisingEvents )
				{
					_webLogger.LogInformation("[DiskWatcher] I'm Back!");
				}

				return true;
			}
			catch
			{
				_webLogger.LogInformation(
					$"[DiskWatcher] next retry {i} - wait for {milliSecondsTimeout}ms");
				// Sleep for a bit; otherwise, it takes a bit of
				// processor time
				Thread.Sleep(milliSecondsTimeout);
				i++;
			}
		}

		_webLogger.LogError($"[DiskWatcher] Failed after {i} times - so stop trying");
		return false;
	}

	private static string DateTimeDebug()
	{
		return ": " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss",
			CultureInfo.InvariantCulture);
	}

	// Define the event handlers.

	/// <summary>
	///     Specify what is done when a file is changed. e.FullPath
	/// </summary>
	/// <param name="source"></param>
	/// <param name="e"></param>
	internal void OnChanged(object source, FileSystemEventArgs e)
	{
		if ( e.FullPath.EndsWith(".tmp") ||
		     !ExtensionRolesHelper.IsExtensionSyncSupported(e.FullPath) )
		{
			return;
		}

		_webLogger.LogDebug($"[DiskWatcher] " +
		                    $"{e.FullPath} OnChanged ChangeType is: {e.ChangeType} " +
		                    DateTimeDebug());

		_queueProcessor.QueueInput(e.FullPath, null, e.ChangeType).ConfigureAwait(false);
		// Specify what is done when a file is changed, created, or deleted.
	}

	private static FileAttributes? GetFileAttributes(string fullPath)
	{
		try
		{
			return File.GetAttributes(fullPath);
		}
		catch ( Exception )
		{
			return null;
		}
	}

	/// <summary>
	///     Specify what is done when a file is renamed. e.OldFullPath to e.FullPath
	/// </summary>
	/// <param name="source">object source (ignored)</param>
	/// <param name="e">arguments</param>
	internal void OnRenamed(object source, RenamedEventArgs e)
	{
		_webLogger.LogInformation($"[DiskWatcher] {e.OldFullPath} OnRenamed to: {e.FullPath}" +
		                          DateTimeDebug());

		var fileAttributes = GetFileAttributes(e.FullPath);
		var isDirectory = fileAttributes == FileAttributes.Directory;

		var isOldFullPathTempFile =
			e.OldFullPath.Contains(Path.DirectorySeparatorChar + "tmp.{")
			|| e.OldFullPath.EndsWith(".tmp");
		var isNewFullPathTempFile = e.FullPath.Contains(Path.DirectorySeparatorChar + "tmp.{")
		                            || e.FullPath.EndsWith(".tmp");
		if ( !isDirectory && isOldFullPathTempFile )
		{
			_queueProcessor.QueueInput(e.FullPath, null, WatcherChangeTypes.Created)
				.ConfigureAwait(false);
			return;
		}

		if ( !isDirectory && isNewFullPathTempFile )
		{
			return;
		}

		_queueProcessor.QueueInput(e.OldFullPath, e.FullPath, WatcherChangeTypes.Renamed)
			.ConfigureAwait(false);
	}
}
