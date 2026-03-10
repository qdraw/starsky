using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starskytest.FakeMocks;

public class FakeIStorageTransient : IStorage
{
	private readonly bool _filesAppearOnSecondCall;

	private readonly bool _folderAppearsOnSecondCall;
	private readonly bool _subdirsAppearOnSecondCall;
	private int _existFolderCalls;
	private int _getDirCalls;
	private int _getFilesCalls;

	public FakeIStorageTransient(bool folderAppearsOnSecondCall = false,
		bool subdirsAppearOnSecondCall = false, bool filesAppearOnSecondCall = false)
	{
		_folderAppearsOnSecondCall = folderAppearsOnSecondCall;
		_subdirsAppearOnSecondCall = subdirsAppearOnSecondCall;
		_filesAppearOnSecondCall = filesAppearOnSecondCall;
	}

	public bool ExistFile(string path)
	{
		return false;
	}

	public bool ExistFolder(string path)
	{
		_existFolderCalls++;
		if ( _existFolderCalls >= 2 && _folderAppearsOnSecondCall )
		{
			return true;
		}

		return false;
	}

	public bool IsFolderEmpty(string path)
	{
		throw new NotImplementedException();
	}

	public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
	{
		return FolderOrFileModel.FolderOrFileTypeList.Deleted;
	}

	public void FolderMove(string fromPath, string toPath)
	{
		throw new NotImplementedException();
	}

	public bool FileMove(string fromPath, string toPath)
	{
		throw new NotImplementedException();
	}

	public void FileCopy(string fromPath, string toPath)
	{
		throw new NotImplementedException();
	}

	public bool FileDelete(string path)
	{
		throw new NotImplementedException();
	}

	public bool CreateDirectory(string path)
	{
		throw new NotImplementedException();
	}

	public bool FolderDelete(string path)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetAllFilesInDirectory(string path)
	{
		_getFilesCalls++;
		if ( _getFilesCalls >= 2 && _filesAppearOnSecondCall )
		{
			return new List<string> { path + "/file1.jpg" };
		}

		return new List<string>();
	}

	public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
	{
		return new List<string>();
	}

	public IEnumerable<string> GetDirectories(string path)
	{
		return new List<string>();
	}

	public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path)
	{
		_getDirCalls++;
		if ( _getDirCalls >= 2 && _subdirsAppearOnSecondCall )
		{
			return new List<KeyValuePair<string, DateTime>>
			{
				new KeyValuePair<string, DateTime>(path + "/sub", DateTime.Now)
			};
		}

		return new List<KeyValuePair<string, DateTime>>();
	}

	Stream IStorage.ReadStream(string path, int maxRead)
	{
		return ReadStream(path, maxRead);
	}

	bool IStorage.WriteStream(Stream stream, string path)
	{
		return WriteStream(stream, path);
	}

	bool IStorage.WriteStreamOpenOrCreate(Stream stream, string path)
	{
		return WriteStreamOpenOrCreate(stream, path);
	}

	Task<bool> IStorage.WriteStreamAsync(Stream stream, string path)
	{
		return WriteStreamAsync(stream, path);
	}

	public Stream ReadStream(string path, int maxRead = -1)
	{
		return Stream.Null;
	}

	public bool WriteStream(Stream stream, string path)
	{
		throw new NotImplementedException();
	}

	public bool WriteStreamOpenOrCreate(Stream stream, string path)
	{
		throw new NotImplementedException();
	}

	public Task<bool> WriteStreamAsync(Stream stream, string path)
	{
		throw new NotImplementedException();
	}

	public StorageInfo Info(string path)
	{
		return new StorageInfo { IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted };
	}

	public bool IsFileReady(string path)
	{
		return true;
	}

	public IAsyncEnumerable<string> ReadLinesAsync(string path, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
