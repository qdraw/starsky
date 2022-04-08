
# Add Migrations
This document describes how to add a migration to the application.

## Install Dotnet EF as global installer
```bash
dotnet tool install -g dotnet-ef
```

## Or update to latest version
```bash
dotnet tool update --global dotnet-ef
```

## Set constance for EF Core
Define constance in `starsky.foundation.database.csproj`
```xml
        <DefineConstants>SYSTEM_TEXT_ENABLED;ENABLE_DEFAULT_DATABASE</DefineConstants>
```


## Run Migration
```bash
cd starsky/starsky.foundation.database
dotnet ef --startup-project ../starsky/starsky.csproj --project starsky.foundation.database.csproj migrations add test
```

## Instead of setting constance (is replaced by defined constance)

(Optional) : Copy code : SetupDatabaseTypes.cs

```c#
			// dirty hack
			_services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlite(_appSettings.DatabaseConnection, 
					b =>
					{
						if (! string.IsNullOrWhiteSpace(foundationDatabaseName) )
						{
							b.MigrationsAssembly(foundationDatabaseName);
						}
					}));
```