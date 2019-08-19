
dotnet tool install -g dotnet-ef

# from starskycore folder

```bash
cd starsky/starskycore
dotnet ef --startup-project ../starsky/starsky.csproj --project starskycore.csproj migrations add test
```
