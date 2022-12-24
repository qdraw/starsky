using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.metathumbnail.Services;

[Service(typeof(IMetaUpdateStatusThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MetaUpdateStatusThumbnailService : IMetaUpdateStatusThumbnailService
{
	private readonly IQuery _query;

	public MetaUpdateStatusThumbnailService(IQuery query)
	{
		_query = query;
	}
	
	public async Task UpdateStatusThumbnail(List<(bool, string)> statusList)
	{
		// ok case
		var itemsSucceed = await _query.GetAllObjectsAsync(statusList.Where(status => 
			status.Item1).Select(p => p.Item2).ToList());
		foreach ( var item in itemsSucceed )
		{
			//item.ThumbnailSizes.Remove(ThumbnailSize.ErrorTinyMeta);
			item.ThumbnailSizes.Add(ThumbnailSize.TinyMeta);
		}
		
		// Error case
		var paths = statusList.Where(status =>
			!status.Item1).Select(p => p.Item2).ToList();
		var itemsFailed = await _query.GetAllObjectsAsync(paths);
		foreach ( var item in itemsFailed )
		{
			//item.ThumbnailSizes.Remove(ThumbnailSize.TinyMeta);
			item.ThumbnailSizes.Add(ThumbnailSize.ErrorTinyMeta);
		}
		
		await _query.UpdateItemAsync(itemsFailed);

		Console.WriteLine();
		// var items = itemsSucceed.Concat(itemsFailed).ToList();
		//
		// foreach ( var chunk in items.Chunk(40) )
		// {
		// 	await _query.UpdateItemAsync(chunk.ToList());
		// 	Console.WriteLine("---1");
		// }
		
	}

}
