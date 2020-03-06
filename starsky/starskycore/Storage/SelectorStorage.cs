using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starskycore.Interfaces;
using starskycore.Storage;

namespace starskycore.Services
{
	[Service(typeof(ISelectorStorage), InjectionLifetime = InjectionLifetime.Scoped)]
	public class SelectorStorage : ISelectorStorage
	{
		private IServiceProvider _serviceProvider;

		public SelectorStorage(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		
		public enum StorageServices
		{
			SubPath,
			Temp,
			Thumbnail,
			/// <summary>
			/// Use only to import from
			/// </summary>
			HostFilesystem 
		}
		
		public IStorage Get(StorageServices storageServices)
		{
			var services = _serviceProvider.GetServices<IStorage>();
			switch ( storageServices )
			{
				case StorageServices.SubPath:
					return services.First(o => o.GetType() == typeof(StorageSubPathFilesystem));
				case StorageServices.Temp:
					return services.First(o => o.GetType() == typeof(StorageTempFilesystem));
				case StorageServices.Thumbnail:
					return services.First(o => o.GetType() == typeof(StorageThumbnailFilesystem));
				case StorageServices.HostFilesystem:
					return services.First(o => o.GetType() == typeof(StorageHostFullPathFilesystem));

				default:
					throw new ArgumentOutOfRangeException(nameof(storageServices), storageServices, null);
			}
		}
	}
}
