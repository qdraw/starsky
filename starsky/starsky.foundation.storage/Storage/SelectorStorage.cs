using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage
{
	[Service(typeof(ISelectorStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class SelectorStorage : ISelectorStorage
	{
		private readonly IServiceProvider _serviceProvider;

		public SelectorStorage(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public enum StorageServices
		{
			SubPath,
			Thumbnail,
			/// <summary>
			/// Use only to import from
			/// </summary>
			HostFilesystem
		}

		public IStorage Get(StorageServices storageServices)
		{
			var services = _serviceProvider.GetServices<IStorage>();
			return storageServices switch
			{
				StorageServices.SubPath => services.First(o =>
					o is StorageSubPathFilesystem),
				StorageServices.HostFilesystem => services.First(o =>
					o is StorageHostFullPathFilesystem),
				StorageServices.Thumbnail => services.First(o =>
					o is StorageThumbnailFilesystem),
				_ => throw new ArgumentOutOfRangeException(
					nameof(storageServices), storageServices, null)
			};
		}
	}
}
