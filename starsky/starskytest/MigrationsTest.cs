using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;

namespace starskytest;

[TestClass]
public sealed class MigrationsTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task MigrationsTest_contextDatabaseMigrate()
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync(TestContext.CancellationToken);
		var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(connection)
			.Options;

		var dbContext = new ApplicationDbContext(contextOptions);

		await dbContext.Database.MigrateAsync(TestContext.CancellationToken);

		// Assert that at least one expected table exists after migration
		await using (var command = dbContext.Database.GetDbConnection().CreateCommand())
		{
			command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='FileIndex';";
			if (command.Connection?.State != System.Data.ConnectionState.Open)
			{
				await command.Connection!.OpenAsync(TestContext.CancellationToken);
			}

			var result = await command.ExecuteScalarAsync(TestContext.CancellationToken);
			Assert.IsNotNull(result, "Expected the FileIndex table to exist after migration.");
		}

		await dbContext.Database.EnsureDeletedAsync(TestContext.CancellationToken);
		await dbContext.DisposeAsync();
	}
}
