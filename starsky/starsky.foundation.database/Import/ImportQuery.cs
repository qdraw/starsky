using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Extensions;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;

namespace starsky.foundation.database.Import
{
	[Service(typeof(IImportQuery), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ImportQuery : IImportQuery
	{
		private readonly bool _isConnection;
		private readonly IServiceScopeFactory _scopeFactory;

		/// <summary>
		/// Query Already imported Database
		/// inject a scope to:
		/// @see: https://docs.microsoft.com/nl-nl/ef/core/miscellaneous/configuring-dbcontext#avoiding-dbcontext-threading-issues
		/// </summary>
		/// <param name="scopeFactory">to avoid threading issues with DbContext</param>
		public ImportQuery(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
			_isConnection = TestConnection();
		}

		/// <summary>
		/// Test if the database connection is there
		/// </summary>
		/// <returns>successful database connection</returns>
		public bool TestConnection()
		{

			var dbContext = new InjectServiceScope(null,_scopeFactory).Context();
			return !_isConnection ? dbContext.TestConnection() : _isConnection;
		}

		public async Task<bool> IsHashInImportDbAsync(string fileHashCode)
		{
			var dbContext = new InjectServiceScope(null, _scopeFactory).Context();

			if ( _isConnection )
			{
				var value = await dbContext.ImportIndex.CountAsync(p => 
					p.FileHash == fileHashCode) != 0; 			// there is no any in ef core
				return value;
			}

			// When there is no mysql connection continue
			Console.WriteLine($">> _isConnection == false");
			return false;
		}

		/// <summary>
		/// Add a new item to the imported database
		/// </summary>
		/// <param name="updateStatusContent">import database item</param>
		/// <returns>fail or success</returns>
		public async Task<bool> AddAsync(ImportIndexItem updateStatusContent)
		{
			var dbContext = new InjectServiceScope(null, _scopeFactory).Context();

			updateStatusContent.AddToDatabase = DateTime.UtcNow;
			await dbContext.ImportIndex.AddAsync(updateStatusContent);
			await dbContext.SaveChangesAsync();
			Console.Write("⬇️");
			// removed MySqlException catch
			return true;
		}
		
		/// <summary>
		/// Get imported items for today
		/// </summary>
		/// <returns>List of items</returns>
		public List<ImportIndexItem> History()
		{
			var dbContext = new InjectServiceScope(null, _scopeFactory).Context();
			return dbContext.ImportIndex.Where(p => p.AddToDatabase >= DateTime.Today).ToList();
			// for debug: p.AddToDatabase >= DateTime.UtcNow.AddDays(-2) && p.Id % 6 == 1
		}
	}
}
