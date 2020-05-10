using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.database.Query
{
	public class InjectServiceScope
	{
		private readonly ApplicationDbContext _dbContext;

		public InjectServiceScope(IServiceScopeFactory scopeFactory)
		{
			if (scopeFactory == null) return;
			var scope = scopeFactory.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		}
		
		/// <summary>
		/// Dependency injection, used in background tasks
		/// </summary>
		internal ApplicationDbContext Context()
		{
			return _dbContext;
		}
	}
}
