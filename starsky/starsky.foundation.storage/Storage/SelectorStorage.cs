using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage;

[Service(typeof(ISelectorStorage), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class SelectorStorage : ISelectorStorage
{
	public enum StorageServices
	{
		/// <summary>
		///     Storage location
		/// </summary>
		SubPath,

		/// <summary>
		///     Location for thumbnails
		/// </summary>
		Thumbnail,

		/// <summary>
		///     Use only to import from
		/// </summary>
		HostFilesystem
	}

	private readonly IEnumerable<IStorage> _services;

	public SelectorStorage(IServiceScopeFactory scopeFactory)
	{
		var scope = scopeFactory.CreateScope();
		_services = scope.ServiceProvider.GetServices<IStorage>();
	}

	public IStorage Get(StorageServices storageServices)
	{
		return storageServices switch
		{
			StorageServices.SubPath => _services.First(o =>
				o is StorageSubPathFilesystem),
			StorageServices.HostFilesystem => _services.First(o =>
				o is StorageHostFullPathFilesystem),
			StorageServices.Thumbnail => _services.First(o =>
				o is StorageThumbnailFilesystem),
			_ => throw new ArgumentOutOfRangeException(
				nameof(storageServices), storageServices, null)
		};
	}
}
