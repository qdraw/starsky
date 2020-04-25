using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

namespace starsky.foundation.database.Query
{
	public class InjectServiceScope
	{
		private readonly ApplicationDbContext _dbContext;

		public InjectServiceScope(ApplicationDbContext context, IServiceScopeFactory scopeFactory)
		{
			if ( context != null )
			{
				_dbContext = context;
				return;
			}
			
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
