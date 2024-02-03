using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starskytest.FakeMocks;

public class FakeIStorage : IStorage
{
	private readonly List<string?> _outputSubPathFolders =
		new List<string?>();

	private readonly List<string?>
		_outputSubPathFiles = new List<string?>();

	private readonly Dictionary<string, DateTime>? _lastEditDict =
		new Dictionary<string, DateTime>();

	private readonly Dictionary<string, byte[]?> _byteList =
		new Dictionary<string, byte[]?>();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="outputSubPathFolders">/</param>
	/// <param name="outputSubPathFiles">/test.jpg</param>
	/// <param name="byteListSource"></param>
	/// <param name="lastEdited"></param>
	public FakeIStorage(List<string>? outputSubPathFolders = null,
		List<string>? outputSubPathFiles = null,
		IReadOnlyList<byte[]?>? byteListSource = null,
		IReadOnlyList<DateTime>? lastEdited = null)
	{
		if ( outputSubPathFolders != null )
		{
			foreach ( var subPath in outputSubPathFolders )
			{
				_outputSubPathFolders.Add(subPath);
			}
		}

		if ( outputSubPathFiles != null )
		{
			foreach ( var subPath in outputSubPathFiles )
			{
				_outputSubPathFiles.Add(subPath);
			}
		}

		if ( byteListSource != null &&
		     byteListSource.Count == _outputSubPathFiles.Count )
		{
			for ( int i = 0; i < _outputSubPathFiles.Count; i++ )
			{
				_byteList.Add(_outputSubPathFiles[i]!, byteListSource[i]);
			}
		}

		if ( lastEdited != null && lastEdited.Any() )
		{
			for ( var i = 0; i < _outputSubPathFiles.Count; i++ )
			{
				_lastEditDict.Add(_outputSubPathFiles[i]!, lastEdited[i]);
			}
		}
	}

	public FakeIStorage(Exception exception)
	{
		_exception = exception;
	}


	private readonly Exception? _exception;
	public int ExceptionCount { get; set; }

	public bool ExistFile(string path)
	{
		return _outputSubPathFiles.Contains(path);
	}

	public bool ExistFolder(string path)
	{
		if ( _exception == null )
		{
			return _outputSubPathFolders.Contains(path);
		}

		ExceptionCount++;
		throw _exception;
	}

	public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(
		string path)
	{
		if ( ExistFile(path) )
		{
			return FolderOrFileModel.FolderOrFileTypeList.File;
		}

		if ( ExistFolder(path) )
		{
			return FolderOrFileModel.FolderOrFileTypeList.Folder;
		}

		return FolderOrFileModel.FolderOrFileTypeList.Deleted;
	}

	/// <summary>
	/// Mock of Move folder
	/// </summary>
	/// <param name="fromPath">subPath from location</param>
	/// <param name="toPath">to SubPath location</param>
	/// <exception cref="ArgumentException">not existing</exception>
	public void FolderMove(string fromPath, string toPath)
	{
		var indexOfFolders = _outputSubPathFolders.IndexOf(fromPath);
		if ( indexOfFolders == -1 )
		{
			throw new ArgumentException(
				$"inputSubPath:{fromPath} - toSubPath:{toPath} indexOfFolders---1");
		}

		_outputSubPathFolders[indexOfFolders] = toPath;
	}

	public void FileMove(string fromPath, string toPath)
	{
		var existOldFile = ExistFile(fromPath);
		var existNewFile = ExistFile(toPath);

		if ( !existOldFile || existNewFile )
		{
			return;
		}

		var indexOfFiles = _outputSubPathFiles.IndexOf(fromPath);
		_outputSubPathFiles[indexOfFiles] = toPath;
	}

	public void FileCopy(string fromPath, string toPath)
	{
		if ( !ExistFile(fromPath) ) return;

		_outputSubPathFiles.Add(toPath);
		_byteList.Add(toPath, _byteList[fromPath]);
	}

	public bool FileDelete(string path)
	{
		if ( !ExistFile(path) ) return false;
		var index = _outputSubPathFiles.IndexOf(path);
		_outputSubPathFiles[index] = null!;
		return true;
	}

	public void CreateDirectory(string path)
	{
		_outputSubPathFolders.Add(path);
	}

	public bool FolderDelete(string path)
	{
		if ( !ExistFolder(path) ) return false;
		var index = _outputSubPathFolders.IndexOf(path);
		_outputSubPathFolders[index] = null;

		// recursive delete all files
		for ( var i = 0; i < _outputSubPathFolders.Count; i++ )
		{
			if ( _outputSubPathFolders[i] != null &&
			     _outputSubPathFolders[i]!.StartsWith(path) )
			{
				_outputSubPathFolders[i] = null;
			}
		}

		return true;
	}

	public IEnumerable<string> GetAllFilesInDirectory(string? path)
	{
		if ( path == null )
		{
			// for thumbnails
			return _outputSubPathFiles
				.Where(p => p?.StartsWith('/') == false).Cast<string>();
		}

		var pathNoEndSlash = PathHelper.RemoveLatestSlash(path);

		// non recursive
		if ( pathNoEndSlash != string.Empty &&
		     !ExistFolder(pathNoEndSlash) )
		{
			return new List<string>();
		}


		return _outputSubPathFiles.Where(p =>
				CheckAndFixParentFiles(pathNoEndSlash, p!))
			.AsEnumerable().Cast<string>();
	}

	public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
	{
		path = PathHelper.RemoveLatestSlash(path);
		return _outputSubPathFiles
			.Where(p => p != null && p.StartsWith(path)).Cast<string>();
	}

	/// <summary>
	/// Should output: /2020/01/2020_01_01 and /2020/01/2020_01_01 test
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public IEnumerable<string> GetDirectories(string path)
	{
		path = PathHelper.RemoveLatestSlash(path);
		var folderFileList = _outputSubPathFolders
			.Where(p => p?.Contains(path) == true && p != path).ToList();

		var parentPathWithSlash = string.IsNullOrEmpty(path) ? "/" : path;

		var folderFileListNotRecursive = folderFileList.Where(
			p => CheckAndFixChildFolders(parentPathWithSlash, p!)).ToList();
		return folderFileListNotRecursive.Cast<string>();
	}

	private static bool CheckAndFixChildFolders(string parentFolder,
		string childFolder)
	{
		var replaced = childFolder.Replace(parentFolder, childFolder);
		if ( replaced.Contains('/') ||
		     replaced.Contains(Path.DirectorySeparatorChar) )
		{
			return true;
		}

		return false;
	}

	private static bool CheckAndFixParentFiles(string parentFolder,
		string filePath)
	{
		if ( parentFolder != string.Empty &&
		     !filePath.StartsWith(parentFolder) ) return false;
		// unescaped: (\/|\\)\w+.[a-z]{1,4}$
		return Regex.Match(filePath,
				$"^{Regex.Escape(parentFolder)}" +
				"(\\/|\\\\)\\w+.[a-z]{1,4}$")
			.Success;
	}

	/// <summary>
	/// Returns a list of directories // Get list of child folders
	/// </summary>
	/// <param name="path">subPath</param>
	/// <returns>list of paths</returns>
	[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
	public IEnumerable<KeyValuePair<string, DateTime>>
		GetDirectoryRecursive(string path)
	{
		if ( path != "/" ) path = PathHelper.RemoveLatestSlash(path);

		if ( !ExistFolder(path) )
		{
			return new List<KeyValuePair<string, DateTime>>();
		}

		var result = new List<KeyValuePair<string, DateTime>>();
		foreach ( var item in _outputSubPathFolders.Where(p =>
				         p?.StartsWith(path) == true && p != path)
			         .AsEnumerable() )
		{
			result.Add(
				new KeyValuePair<string, DateTime>(item!, DateTime.Now));
		}

		return result;
	}

	/// <summary>
	/// Read Stream (and keep open)
	/// </summary>
	/// <param name="path">location</param>
	/// <param name="maxRead">how many bytes are read (default all or -1)</param>
	/// <returns>Stream with data (non-disposed)</returns>
	public Stream ReadStream(string path, int maxRead = -1)
	{
		if ( _exception != null )
		{
			ExceptionCount++;
			throw _exception;
		}

		if ( !ExistFile(path) )
		{
			return Stream.Null;
		}

		// whats that? -->
		if ( ExistFile(path) && _byteList.All(p => p.Key != path) )
		{
			var byteArray = Encoding.UTF8.GetBytes("test");
			var stream = new MemoryStream(byteArray);
			return stream;
		}

		var byteListByPath = _byteList.FirstOrDefault(p => p.Key == path).Value;
		if ( byteListByPath == null )
		{
			return Stream.Null;
		}

		var returnStream = new MemoryStream(byteListByPath);
		return returnStream;
	}

	public bool WriteStreamOpenOrCreate(Stream stream, string path)
	{
		if ( !_outputSubPathFiles.Contains(path) )
		{
			_outputSubPathFiles.Add(path);
		}

		stream.Seek(0, SeekOrigin.Begin);

		using ( var memoryStream = new MemoryStream() )
		{
			stream.CopyTo(memoryStream);
			var byteArray = memoryStream.ToArray();

			if ( _byteList.Any(p => p.Key == path) )
			{
				_byteList[path] = byteArray;
				return true;
			}

			_byteList.Add(path, byteArray);
			if ( byteArray.Length == 0 )
				throw new FileLoadException(
					$"FakeIStorage WriteStream => path {path} is 0 bytes");
		}

		return true;
	}

	public bool WriteStream(Stream stream, string path)
	{
		ArgumentNullException.ThrowIfNull(path);

		_outputSubPathFiles.Add(path);

		if ( stream.CanSeek )
		{
			stream.Seek(0, SeekOrigin.Begin);
		}
		else
		{
			Console.WriteLine("FakeIStorage WriteStream => stream can't seek");
		}

		using ( var ms = new MemoryStream() )
		{
			stream.CopyTo(ms);
			var byteArray = ms.ToArray();

			if ( _byteList.Any(p => p.Key == path) )
			{
				_byteList[path] = byteArray;
				return true;
			}

			_byteList.Add(path, byteArray);
			if ( byteArray.Length == 0 )
			{
				throw new FileLoadException(
					$"FakeIStorage WriteStream => path {path} is 0 bytes");
			}
		}

		stream.Dispose();
		return true;
	}

	/// <summary>
	/// Write and dispose afterwards
	/// </summary>
	/// <param name="stream">stream</param>
	/// <param name="path">where to write to</param>
	/// <returns>is Success</returns>
	public Task<bool> WriteStreamAsync(Stream stream, string path)
	{
		return Task.FromResult(WriteStream(stream, path));
	}

	public virtual StorageInfo Info(string path)
	{
		if ( ExistFolder(path) )
		{
			return new StorageInfo
			{
				IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList
					.Folder
			};
		}

		if ( !ExistFile(path) )
		{
			return new StorageInfo
			{
				IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList
					.Deleted
			};
		}

		var result = _byteList.FirstOrDefault(p => p.Key == path).Value;

		var lastEdit =
			new DateTime(1994, 7, 5, 16, 23, 42, DateTimeKind.Utc);
		if ( _lastEditDict != null )
		{
			lastEdit = _lastEditDict.FirstOrDefault(p => p.Key == path)
				.Value;
		}

		long len = 0;
		if ( result?.Length != null )
		{
			len = result.Length;
		}

		return new StorageInfo
		{
			IsFolderOrFile =
				FolderOrFileModel.FolderOrFileTypeList.File,
			Size = len,
			LastWriteTime = lastEdit
		};
	}

	public DateTime SetLastWriteTime(string path, DateTime? dateTime = null)
	{
		SetDateTime(path, dateTime ?? DateTime.Now);
		return dateTime ?? DateTime.Now;
	}

	private void SetDateTime(string path, DateTime dateTime)
	{
		if ( _lastEditDict == null ) return;
		if ( _lastEditDict.Any(p => p.Key == path) )
		{
			_lastEditDict[path] = dateTime;
			return;
		}

		_lastEditDict.Add(path, dateTime);
	}
}
