using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace starsky.foundation.database.Data;

public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

#if ENABLE_MYSQL_DATABASE
		// dirty hack
		var foundationDatabaseName = typeof(ApplicationDbContext)
			.Assembly.FullName?.Split(",")[0];

		optionsBuilder.UseMySql("Server=localhost;Port=1234;database=test;uid=test;pwd=test;",
			new MariaDbServerVersion("10.2"),
			b => { b.MigrationsAssembly(foundationDatabaseName); });
		return new ApplicationDbContext(optionsBuilder.Options);
#endif

		optionsBuilder.UseSqlite("Data Source=blog.db");

		return new ApplicationDbContext(optionsBuilder.Options);
	}
}
