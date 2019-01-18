using System;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace starskycore.Extensions
{
	public static class EntityFrameworkExtensions
	{
		public static bool TestConnection(this DbContext context)
		{
			DbConnection connection = context.Database.GetDbConnection();

			try
			{
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
