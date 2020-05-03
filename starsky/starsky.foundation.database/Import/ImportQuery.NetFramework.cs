using System;
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
		/// Non-async Add a new item to the imported database
		/// </summary>
		/// <param name="updateStatusContent">import database item</param>
		/// <returns>fail or success</returns>
		public override async Task<bool> AddAsync(ImportIndexItem updateStatusContent)
		{
			var dbContext = new InjectServiceScope(null, _scopeFactory).Context();

			updateStatusContent.AddToDatabase = DateTime.UtcNow;
			// ReSharper disable once MethodHasAsyncOverload
			dbContext.ImportIndex.Add(updateStatusContent);
			// ReSharper disable once MethodHasAsyncOverload
			dbContext.SaveChanges();
			Console.Write(">️"); //arrow down
			// removed MySqlException catch
			return true;
		}
	}
}
