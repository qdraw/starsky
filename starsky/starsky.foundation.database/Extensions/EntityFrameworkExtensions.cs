using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace starsky.foundation.database.Extensions
{
	public static class EntityFrameworkExtensions
	{
		/// <summary>
		/// Test the connection if this is mysql
		/// </summary>
		/// <param name="context">database context</param>
		/// <returns>bool, true if connection is there</returns>
		/// <exception cref="ArgumentNullException">When AppSettings is null</exception>
		public static bool TestConnection(this DbContext context)
		{
			if ( context?.Database == null ) return false;
			
			try
			{
				context.Database.CanConnect();
			}
			catch ( MySqlException e)
			{
				Console.WriteLine($"WARNING >>> \n{e}\n <<<");
				return false;
			}
			return true;
		}
	}
}
