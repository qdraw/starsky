using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
#pragma warning disable CS8618

[assembly: InternalsVisibleTo("starskytest"),
		  InternalsVisibleTo("starsky.foundation.settings")]
namespace starsky.foundation.database.Query
{
	public class InjectServiceScope
	{
		private readonly ApplicationDbContext _dbContext;

		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
		public InjectServiceScope(IServiceScopeFactory? scopeFactory)
		{
			if ( scopeFactory == null )
			{
				return;
			}

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
