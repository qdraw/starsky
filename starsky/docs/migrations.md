
```bash
dotnet tool install -g dotnet-ef
```

# from starsky.foundation.database folder

```bash
dotnet tool update --global dotnet-ef
```

Define constance in `starsky.foundation.database.csproj`
```
        <DefineConstants>SYSTEM_TEXT_ENABLED;ENABLE_DEFAULT_DATABASE</DefineConstants>
```

(Optional) : Copy code : SetupDatabaseTypes.cs
```
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


```bash
cd starsky/starsky.foundation.database
dotnet ef --startup-project ../starsky/starsky.csproj --project starsky.foundation.database.csproj migrations add test
```
