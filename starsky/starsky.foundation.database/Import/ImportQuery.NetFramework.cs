using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
#pragma warning disable 1998

namespace starsky.foundation.database.Import
{
	public class ImportQueryNetFramework: ImportQuery
	{
		private readonly bool _isConnection;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ImportQuery _importQuery;

		/// <summary>
		/// Query Already imported Database
		/// inject a scope to:
		/// @see: https://docs.microsoft.com/nl-nl/ef/core/miscellaneous/configuring-dbcontext#avoiding-dbcontext-threading-issues
		/// </summary>
		/// <param name="scopeFactory">to avoid threading issues with DbContext</param>
		public ImportQueryNetFramework(IServiceScopeFactory scopeFactory) : base(scopeFactory)
		{
			_scopeFactory = scopeFactory;
			_isConnection = TestConnection();
			_importQuery = new ImportQuery(scopeFactory);
		}

		/// <summary>
		/// Non-async method for IsHashInImportDb
		/// </summary>
		/// <param name="fileHashCode"></param>
		/// <returns></returns>
		public override async Task<bool> IsHashInImportDbAsync(string fileHashCode)
		{
			var dbContext = new InjectServiceScope(null, _scopeFactory).Context();

			if ( _isConnection )
			{
				var value = dbContext.ImportIndex.Count(p => 
					p.FileHash == fileHashCode) != 0; 			// there is no any in ef core
				return value;
			}

			// When there is no mysql connection continue
			Console.WriteLine($">> _isConnection == false");
			return false;
		}

		/// <summary>
		/// (Sync) Add Range Item
		/// </summary>
		/// <param name="importIndexItemList">list of items to import</param>
		/// <returns></returns>
		public override async Task<List<ImportIndexItem>> AddRangeAsync(List<ImportIndexItem> importIndexItemList)
		{
			// ReSharper disable once MethodHasAsyncOverload
			return _importQuery.AddRange(importIndexItemList);
		}
	}
}
