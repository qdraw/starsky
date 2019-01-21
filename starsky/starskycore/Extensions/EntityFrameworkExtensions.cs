using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using starskycore.Models;

namespace starskycore.Extensions
{
	public static class EntityFrameworkExtensions
	{
		/// <summary>
		/// Test the connection if this is mysql
		/// </summary>
		/// <param name="context">database context</param>
		/// <param name="appSettings">to know it mysql</param>
		/// <returns>bool, true if connection is there</returns>
		/// <exception cref="ArgumentNullException">When AppSettings is null</exception>
		public static bool TestConnection(this DbContext context, AppSettings appSettings)
		{

			if ( appSettings == null )
				throw new ArgumentNullException(nameof(appSettings));

			// MYSQL only
			if ( appSettings.DatabaseType != AppSettings.DatabaseTypeList.Mysql ) return true;
			
			try
			{					
				DbConnection connection = context.Database.GetDbConnection();
				connection.Open();
				connection.Close();
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
