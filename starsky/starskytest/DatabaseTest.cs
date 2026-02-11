using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;

namespace starskytest;

public class DatabaseTest : IDisposable
{
	private readonly SqliteConnection _connection;
	protected readonly ApplicationDbContext DbContext;

	public DatabaseTest()
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_connection.Open();
		var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(_connection)
			.Options;

		DbContext = new ApplicationDbContext(contextOptions);
		DbContext.Database.EnsureCreated();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if ( !disposing )
		{
			return;
		}

		DbContext.Database.EnsureDeleted();
		DbContext.Dispose();
		_connection.Dispose();
	}
}
