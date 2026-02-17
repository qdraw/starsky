using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

namespace starskytest;

public class DatabaseTest : IDisposable
{
	private readonly SqliteConnection _connection;
	protected readonly ApplicationDbContext DbContext;
	internal readonly IServiceScopeFactory scopeFactory;

	public DatabaseTest()
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_connection.Open();
		var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(_connection)
			.Options;

		DbContext = new ApplicationDbContext(contextOptions);
		DbContext.Database.EnsureCreated();
		
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlite(_connection) );
		services.AddMemoryCache();
		var serviceProvider = services.BuildServiceProvider();
		scopeFactory =  serviceProvider.GetRequiredService<IServiceScopeFactory>();
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
