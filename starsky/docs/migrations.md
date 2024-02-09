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

```bash
# https://www.nuget.org/packages/dotnet-ef#versions-body-tab
dotnet tool update --global dotnet-ef --version 8.0.1
```

## Set constance for EF Core (optional)

Define constance in `starsky.foundation.database.csproj` (to remove it after the migration)

only when used mysql:

```xml

<DefineConstants>ENABLE_MYSQL_DATABASE</DefineConstants>
```

## Run Migration

> Please think about this issue:
> `warning CS8981: The type name 'limitdataprotectionkeyslength' only contains lower-cased ascii characters.` 
> `Such names may become reserved for the language`

Run the following command:

```bash
cd starsky/starsky.foundation.database
dotnet ef --project starsky.foundation.database.csproj migrations add test
```

The migration should be ready :)
You should test **both** with MySQL and SQLite.
For MySql its the easiest to run the database and/or the application with docker-compose.

We use this for the migration:
https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#from-a-design-time-factory

## Remove

remove the following value if added:

```
        <DefineConstants>ENABLE_MYSQL_DATABASE</DefineConstants>
```

## undo latest migration

```bash
cd starsky/starsky.foundation.database
dotnet ef --project starsky.foundation.database.csproj migrations remove --force
```

