using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace starskycore.Extensions
{
	public static class EntityFrameworkExtensions
	{
		public static bool TestConnection(this DbContext context)
		{

			try
			{
				DbConnection connection = context.Database.GetDbConnection();
				connection.Open();
				connection.Close();
			}
			catch (InvalidOperationException) // for tests
			{
				return true;
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
