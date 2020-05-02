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

		private bool IsDisposed(ApplicationDbContext context)
		{
			if ( context == null ) return true;
			try
			{
				context.Database.CanConnect();
				return false;
			}
			catch ( ObjectDisposedException)
			{
				context.Dispose();
				return true;
			}
		}
		
		public InjectServiceScope(ApplicationDbContext context, IServiceScopeFactory scopeFactory)
		{
			if ( !IsDisposed(context))
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
