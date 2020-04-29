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
		private readonly ApplicationDbContext _dbContext;
		private readonly bool _isConnection;

		public ImportQuery(ApplicationDbContext dbContext,
			IServiceScopeFactory scopeFactory = null)
		{
			_dbContext = new InjectServiceScope(dbContext, scopeFactory).Context();
			_isConnection = _dbContext.TestConnection();
		}

		/// <summary>
		/// Test if the database connection is there
		/// </summary>
		/// <returns>successful database connection</returns>
		public bool TestConnection()
		{
			return !_isConnection ? _dbContext.TestConnection() : _isConnection;
		}

		public async Task<bool> IsHashInImportDbAsync(string fileHashCode)
		{

			if ( _isConnection )
				return await _dbContext.ImportIndex.CountAsync(p => 
					       p.FileHash == fileHashCode) != 0; 
			// there is no any in ef core

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
			updateStatusContent.AddToDatabase = DateTime.UtcNow;

			await _dbContext.ImportIndex.AddAsync(updateStatusContent);
			await _dbContext.SaveChangesAsync();
			// removed MySqlException catch
			return true;
		}
		
		/// <summary>
		/// Get imported items for today
		/// </summary>
		/// <returns>List of items</returns>
		public List<ImportIndexItem> History()
		{
			return _dbContext.ImportIndex.Where(p => p.AddToDatabase >= DateTime.Today).ToList();
			// for debug: p.AddToDatabase >= DateTime.UtcNow.AddDays(-2) && p.Id % 6 == 1
		}
	}
}
