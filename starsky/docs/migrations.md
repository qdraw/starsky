
```
cd starsky
```

```
dotnet ef migrations add ColorClassFeature
```

```bash
dotnet ef database update
```

# from starskycore folder
```bash
dotnet ef --startup-project ../starsky/starsky.csproj --project starskycore.csproj migrations add test
```
