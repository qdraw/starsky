using System;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Extensions
{
	public static class EntityFrameworkExtensions
	{
		/// <summary>
		/// Test the connection if this is mysql
		/// </summary>
		/// <param name="context">database context</param>
		/// <param name="logger">logger</param>
		/// <returns>bool, true if connection is there</returns>
		/// <exception cref="ArgumentNullException">When AppSettings is null</exception>
		public static bool TestConnection(this DbContext context, IWebLogger logger)
		{
			if ( context?.Database == null ) return false;
			
			try
			{
				context.Database.CanConnect();
			}
			catch ( MySqlException e)
			{
				logger.LogInformation($"[TestConnection] WARNING >>> \n{e}\n <<<");
				return false;
			}
			return true;
		}
	}
}
