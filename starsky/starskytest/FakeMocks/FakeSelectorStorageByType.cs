using System;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskytest.FakeMocks;

public class FakeSelectorStorageByType : ISelectorStorage
{
	private readonly IStorage _subPathStorage;
	private readonly IStorage _thumbnailStorage;
	private readonly IStorage _hostFilesystemStorage;

	public FakeSelectorStorageByType(IStorage subPathStorage, IStorage thumbnailStorage,
		IStorage hostFilesystemStorage)
	{
		_subPathStorage = subPathStorage;
		_thumbnailStorage = thumbnailStorage;
		_hostFilesystemStorage = hostFilesystemStorage;
	}

	public IStorage Get(SelectorStorage.StorageServices storageServices)
	{
		return storageServices switch
		{
			SelectorStorage.StorageServices.SubPath => _subPathStorage,
			SelectorStorage.StorageServices.HostFilesystem => _hostFilesystemStorage,
			SelectorStorage.StorageServices.Thumbnail => _thumbnailStorage,
			_ => throw new ArgumentOutOfRangeException(nameof(storageServices), storageServices,
				null)
		};
	}
}
