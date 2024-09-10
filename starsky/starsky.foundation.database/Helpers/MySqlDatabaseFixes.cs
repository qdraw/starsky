using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Helpers
{
	public sealed class MySqlDatabaseFixes
	{
		private readonly MySqlConnection? _connection;
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public MySqlDatabaseFixes(MySqlConnection? connection, AppSettings appSettings,
			IWebLogger logger)
		{
			_connection = connection;
			_appSettings = appSettings;
			_logger = logger;
		}

		/// <summary>
		/// Fix the database
		/// To undo:
		/// ALTER DATABASE `database_name` CHARACTER SET latin1 COLLATE latin1_swedish_ci;
		/// 
		/// </summary>
		/// <param name="tableNames"></param>
		/// <returns></returns>
		public async Task<bool?> FixUtf8Encoding(List<string?> tableNames)
		{
			var isUtf8 = await IsUtf8();
			if ( isUtf8 != false )
			{
				return isUtf8;
			}

			await SetDatabaseSettingToUtf8();
			foreach ( var tableName in tableNames )
			{
				await SetTableToUtf8(tableName);
			}

			return true;
		}

		internal async Task<bool?> SetTableToUtf8(string? tableName)
		{
			if ( _connection == null || tableName == null )
			{
				return null;
			}

			var query = "ALTER TABLE `" + tableName + "`" +
						" CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
			await ExecuteNonQueryAsync(query);
			return true;
		}

		internal async Task<bool?> SetDatabaseSettingToUtf8()
		{
			if ( _connection == null )
			{
				return null;
			}

			var query = "ALTER DATABASE `" + _connection.Database + "` " +
						"CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
			await ExecuteNonQueryAsync(query);
			return true;
		}

		internal async Task<int?> ExecuteNonQueryAsync(string query)
		{
			var myCommand = new MySqlCommand(query);
			myCommand.Connection = _connection;
			if ( myCommand.Connection?.State != ConnectionState.Open )
			{
				return null;
			}

			return await myCommand.ExecuteNonQueryAsync();
		}

		internal async Task<bool?> IsUtf8()
		{
			if ( _connection == null )
			{
				return null;
			}

			var query = "SELECT DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME " +
						"FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" +
						_connection.Database + "'; ";
			var command = new MySqlCommand(query, _connection);

			var tableNames = await ReadCommand(command);

			var isUtf8 = tableNames.FirstOrDefault()?.Contains("utf8mb4,utf8mb4_general_ci") ==
						 true;
			return isUtf8;
		}

		public async Task OpenConnection()
		{
			if ( _connection == null ||
				 _appSettings.DatabaseType != AppSettings.DatabaseTypeList.Mysql )
			{
				return;
			}

			if ( _connection.State != ConnectionState.Open )
			{
				try
				{
					await _connection.OpenAsync();
				}
				catch ( MySqlException exception )
				{
					_logger.LogError(
						$"[MySqlDatabaseFixes] OpenAsync MySqlException {exception.Message}",
						exception);
				}
			}
		}

		public async Task<bool?> FixAutoIncrement(string tableName, bool dispose = false)
		{
			if ( _connection == null ||
				 _appSettings.DatabaseType != AppSettings.DatabaseTypeList.Mysql )
			{
				return null;
			}

			var autoIncrementExist = await CheckAutoIncrementExist(tableName);
			if ( autoIncrementExist != false )
			{
				if ( dispose )
				{
					await _connection.DisposeAsync();
				}

				return autoIncrementExist;
			}

			var result = await AlterTableAutoIncrement(tableName);
			if ( dispose )
			{
				await _connection.DisposeAsync();
			}

			return result != null;
		}

		public async Task DisposeAsync()
		{
			if ( _connection == null )
			{
				return;
			}

			await _connection.DisposeAsync();
		}

		internal async Task<bool?> CheckAutoIncrementExist(string tableName,
			string columnName = "Id")
		{
			if ( _connection == null )
			{
				return null;
			}

			var query = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS " +
						"WHERE TABLE_NAME = '" + tableName + "' " +
						"AND COLUMN_NAME = '" + columnName + "' " +
						"AND DATA_TYPE = 'int' " +
						"AND COLUMN_DEFAULT IS NULL " +
						"AND IS_NULLABLE = 'NO' " +
						"AND EXTRA like '%auto_increment%'";
			var command = new MySqlCommand(query, _connection);

			var tableNames = await ReadCommand(command);

			return tableNames.Count == 1;
		}

		private static async Task<List<string>> ReadCommand(MySqlCommand command)
		{
			if ( command.Connection?.State != ConnectionState.Open )
			{
				return [];
			}

			var tableNames = new List<string>();
			await using var reader = await command.ExecuteReaderAsync();
			while ( await reader.ReadAsync() )
			{
				// at least two columns
				tableNames.Add(reader.GetString(0) + "," + reader.GetString(1));
			}

			return tableNames;
		}

		internal async Task<int?> AlterTableAutoIncrement(string tableName,
			string columnName = "Id")
		{
			if ( _connection == null )
			{
				return null;
			}

			var myInsertQuery = "ALTER TABLE `" + tableName + "` MODIFY " + columnName +
								" INTEGER NOT NULL AUTO_INCREMENT;";
			return await ExecuteNonQueryAsync(myInsertQuery);
		}
	}
}
