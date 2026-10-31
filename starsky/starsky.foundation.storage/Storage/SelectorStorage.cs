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
		HostFilesystem,
		
		/// <summary>
		/// Temporary storage 
		/// </summary>
		Temporary
	}

	private readonly IEnumerable<IStorage> _services;

	/// <summary>
	///     Takes the provider of the current scope (not a new scope): the storages must see the
	///     tenant of this request, and a new scope would never be disposed
	/// </summary>
	public SelectorStorage(IServiceProvider serviceProvider)
	{
		_services = serviceProvider.GetServices<IStorage>();
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
			StorageServices.Temporary => _services.First(o =>
				o is StorageTemporaryFilesystem),			
			_ => throw new ArgumentOutOfRangeException(
				nameof(storageServices), storageServices, null)
		};
	}
}
