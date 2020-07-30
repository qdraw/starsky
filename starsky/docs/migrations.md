
```bash
dotnet tool install -g dotnet-ef
```

# from starsky.foundation.database folder

```bash
dotnet tool update --global dotnet-ef
```

```bash
cd starsky/starsky.foundation.database
dotnet ef --startup-project ../starsky/starsky.csproj --project starsky.foundation.database.csproj migrations add test
```
