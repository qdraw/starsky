#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Helpers;

public class FixMySqlAutoIncrement
{
	private readonly MySqlConnection? _connection;
	private readonly AppSettings _appSettings;

	public FixMySqlAutoIncrement(MySqlConnection? connection, AppSettings appSettings)
	{
		_connection = connection;
		_appSettings = appSettings;
		
	}
	
	internal async Task<bool?> AutoIncrement( string tableName)
	{
		if (_connection == null ||  _appSettings.DatabaseType != AppSettings.DatabaseTypeList.Mysql )
		{
			return null;
		}
		
		await _connection.OpenAsync();

		var autoIncrementExist = await CheckAutoIncrementExist(tableName);
		if (autoIncrementExist != false )
		{
			return autoIncrementExist;
		}

		await AlterTable(tableName);
		await _connection.DisposeAsync();
		return true;
	}

	internal async Task<bool?> CheckAutoIncrementExist(string tableName)
	{
		if ( _connection == null ) return null;

		var query = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS " +
		            "WHERE TABLE_NAME = '"+ tableName + "' " +
		            "AND COLUMN_NAME = 'Id' " +
		            "AND DATA_TYPE = 'int' " +
		            "AND COLUMN_DEFAULT IS NULL " +
		            "AND IS_NULLABLE = 'NO' " +
		            "AND EXTRA like '%auto_increment%'";
		await using var command = new MySqlCommand(query, _connection);

		var tableNames = new List<string>();
		await using var reader = await command.ExecuteReaderAsync();
		while (reader.Read())
		{
			tableNames.Add(reader.GetString(0));
		}

		if ( tableNames.Count != 1 ) return true;
		await _connection.DisposeAsync();
		return false;
	}
	
	internal async Task<bool?> AlterTable(string tableName)
	{
		if ( _connection == null ) return null;
		var myInsertQuery = "ALTER TABLE "+ tableName+ " MODIFY Id INTEGER NOT NULL AUTO_INCREMENT;";
		var myCommand = new MySqlCommand(myInsertQuery);
		myCommand.Connection = _connection;
		myCommand.ExecuteNonQuery();
		await myCommand.Connection.CloseAsync();
		return true;
	}

}
