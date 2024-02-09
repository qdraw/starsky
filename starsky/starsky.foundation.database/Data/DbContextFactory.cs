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

			_services.AddDbContext<ApplicationDbContext>(options =>
				options.UseMySql(_appSettings.DatabaseConnection, GetServerVersionMySql(), 
					b =>
					{
						if (! string.IsNullOrWhiteSpace(foundationDatabaseName) )
						{
							b.MigrationsAssembly(foundationDatabaseName);
						}
					}));
#endif

		optionsBuilder.UseSqlite("Data Source=blog.db");

		return new ApplicationDbContext(optionsBuilder.Options);
	}
}
