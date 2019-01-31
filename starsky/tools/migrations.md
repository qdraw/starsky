
cd starsky
dotnet ef migrations add ColorClassFeature

dotnet ef database update


# from starskycore folder
dotnet ef --startup-project ../starsky/starsky.csproj --project starskycore.csproj migrations add test
