using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starskycore.Interfaces;

namespace starskycore.Services
{
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
			Thumbnail
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
				default:
					throw new ArgumentOutOfRangeException(nameof(storageServices), storageServices, null);
			}
		}
	}
}
