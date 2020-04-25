using System;
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

		public async Task<bool> IsHashInImportDb(string fileHashCode)
		{

			if ( _isConnection )
				return await _dbContext.ImportIndex.CountAsync(p => p.FileHash == fileHashCode) != 0; 
			// there is no any in ef core

			// When there is no mysql connection continue
			Console.WriteLine($">> _isConnection == false");
			return false;
		}
	}
}
