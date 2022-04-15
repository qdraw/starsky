#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Helpers;

public class MySqlDatabaseFixes
{
	private readonly MySqlConnection? _connection;
	private readonly AppSettings _appSettings;

	public MySqlDatabaseFixes(MySqlConnection? connection, AppSettings appSettings)
	{
		_connection = connection;
		_appSettings = appSettings;
		
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
		if (isUtf8 != false ) return isUtf8;
		await SetDatabaseSettingToUtf8();
		foreach ( var tableName in tableNames )
		{
			await SetTableToUtf8(tableName);
		}
		return true;
	}

	internal async Task<bool?> SetTableToUtf8(string? tableName)
	{
		if ( _connection == null || tableName == null) return null;
		var query = "ALTER TABLE `" + tableName + "`" +
			" CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
		await ExecuteNonQueryAsync(query);
		return true;
	}

	internal async Task<bool?> SetDatabaseSettingToUtf8()
	{
		if ( _connection == null ) return null;
		var query = "ALTER DATABASE `" + _connection.Database + "` " +
		            "CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
		await ExecuteNonQueryAsync(query);
		return true;
	}

	internal async Task ExecuteNonQueryAsync(string query)
	{
		var myCommand = new MySqlCommand(query);
		myCommand.Connection = _connection;
		await myCommand.ExecuteNonQueryAsync();
	}

	internal async Task<bool?> IsUtf8()
	{
		if ( _connection == null ) return null;

		var query = "SELECT DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME " +
		            "FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '"+ _connection.Database + "'; ";
		var command = new MySqlCommand(query, _connection);
		
		var tableNames = await ReadCommand(command);

		var isUtf8 =  tableNames.FirstOrDefault()?.Contains( "utf8mb4,utf8mb4_general_ci") == true;
		return isUtf8;
	}

	public async Task OpenConnection()
	{
		if (_connection == null ||  _appSettings.DatabaseType != AppSettings.DatabaseTypeList.Mysql )
		{
			return;
		}
		await _connection.OpenAsync();
	}

	public async Task<bool?> FixAutoIncrement( string tableName)
	{
		if (_connection == null ||  _appSettings.DatabaseType != AppSettings.DatabaseTypeList.Mysql )
		{
			return null;
		}
		

		var autoIncrementExist = await CheckAutoIncrementExist(tableName);
		if (autoIncrementExist != false )
		{
			await _connection.DisposeAsync();
			return autoIncrementExist;
		}

		await AlterTableAutoIncrement(tableName);
		await _connection.DisposeAsync();
		return true;
	}

	internal async Task<bool?> CheckAutoIncrementExist(string tableName, string columnName = "Id")
	{
		if ( _connection == null ) return null;

		var query = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS " +
		            "WHERE TABLE_NAME = '"+ tableName + "' " +
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
		var tableNames = new List<string>();
		await using var reader = await command.ExecuteReaderAsync();
		while (reader.Read())
		{
			// at least two columns
			tableNames.Add(reader.GetString(0) + "," + reader.GetString(1));
		}
		return tableNames;
	}
	
	internal async Task<bool?> AlterTableAutoIncrement(string tableName, string columnName = "Id")
	{
		if ( _connection == null ) return null;
		var myInsertQuery = "ALTER TABLE `"+ tableName+ "` MODIFY " + columnName + " INTEGER NOT NULL AUTO_INCREMENT;";
		await ExecuteNonQueryAsync(myInsertQuery);
		return true;
	}

}
