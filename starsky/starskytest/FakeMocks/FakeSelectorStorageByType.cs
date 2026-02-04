using System;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskytest.FakeMocks;

public class FakeSelectorStorageByType : ISelectorStorage
{
	private readonly IStorage _hostFilesystemStorage;
	private readonly IStorage _subPathStorage;
	private readonly IStorage _tempStorage;
	private readonly IStorage _thumbnailStorage;

	public FakeSelectorStorageByType(IStorage subPathStorage, IStorage thumbnailStorage,
		IStorage hostFilesystemStorage, IStorage tempStorage)
	{
		_subPathStorage = subPathStorage;
		_thumbnailStorage = thumbnailStorage;
		_hostFilesystemStorage = hostFilesystemStorage;
		_tempStorage = tempStorage;
	}

	public IStorage Get(SelectorStorage.StorageServices storageServices)
	{
		return storageServices switch
		{
			SelectorStorage.StorageServices.SubPath => _subPathStorage,
			SelectorStorage.StorageServices.HostFilesystem => _hostFilesystemStorage,
			SelectorStorage.StorageServices.Thumbnail => _thumbnailStorage,
			SelectorStorage.StorageServices.Temporary => _tempStorage,
			_ => throw new ArgumentOutOfRangeException(nameof(storageServices), storageServices,
				null)
		};
	}
}
