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
		/// <returns>bool, true if connection is there</returns>
		/// <exception cref="ArgumentNullException">When AppSettings is null</exception>
		public static bool TestConnection(this DbContext context)
		{
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
